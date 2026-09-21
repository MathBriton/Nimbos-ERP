using Nimbus.Domain.Common;
using Nimbus.Domain.ObjetosDeValor;

namespace Nimbus.Domain.Entidades;

/// <summary>
/// Cliente do ERP. Primeiro cadastro completo do sistema e o modelo que os
/// demais (Fornecedor, Produto) replicam.
/// </summary>
public class Cliente : EntidadeBase
{
    // Construtor sem parametros exigido pelo EF Core para materializar a entidade.
    private Cliente()
    {
        Nome = string.Empty;
        Documento = string.Empty;
    }

    /// <summary>Nome da pessoa fisica ou razao social da juridica.</summary>
    public string Nome { get; private set; }

    /// <summary>Nome fantasia. So faz sentido para pessoa juridica.</summary>
    public string? NomeFantasia { get; private set; }

    /// <summary>
    /// CPF ou CNPJ, somente digitos.
    ///
    /// Guardado como texto, e nao como o objeto de valor <see cref="ObjetosDeValor.Documento"/>:
    /// mapeado por conversor, o EF Core o trataria como opaco e nao conseguiria
    /// traduzir LIKE nem ORDER BY sobre ele - e busca parcial por documento e
    /// justamente um dos filtros da listagem. O objeto de valor continua sendo
    /// o unico caminho de entrada: nada escreve aqui sem passar por
    /// <see cref="ObjetosDeValor.Documento.Criar"/>, que valida os digitos
    /// verificadores e normaliza.
    /// </summary>
    public string Documento { get; private set; }

    /// <summary>
    /// Redundante com o tamanho do documento, mas mapeada como coluna propria
    /// para que a listagem possa filtrar por tipo de pessoa em SQL. Sempre
    /// escrita junto com o documento, para nao divergir.
    /// </summary>
    public TipoDePessoa TipoDePessoa { get; private set; }

    public string? Email { get; private set; }

    /// <summary>Somente digitos, com DDD.</summary>
    public string? Telefone { get; private set; }

    public Endereco? Endereco { get; private set; }

    /// <summary>Anotacoes livres sobre o cliente.</summary>
    public string? Observacoes { get; private set; }

    public bool Ativo { get; private set; } = true;

    public static Cliente Criar(
        string nome,
        TipoDePessoa tipoDePessoa,
        string documento,
        string? nomeFantasia = null,
        string? email = null,
        string? telefone = null,
        Endereco? endereco = null,
        string? observacoes = null)
    {
        var documentoValidado = ObjetosDeValor.Documento.Criar(documento, tipoDePessoa);

        var cliente = new Cliente
        {
            Documento = documentoValidado.Numero,
            TipoDePessoa = documentoValidado.TipoDePessoa,
            Ativo = true,
        };

        cliente.DefinirDadosCadastrais(nome, nomeFantasia, observacoes);
        cliente.DefinirContato(email, telefone);
        cliente.DefinirEndereco(endereco);

        return cliente;
    }

    public void DefinirDadosCadastrais(string nome, string? nomeFantasia, string? observacoes)
    {
        Nome = ExcecaoDeDominio.TextoObrigatorio(nome, "Nome");

        ExcecaoDeDominio.SeVerdadeiro(
            Nome.Length > 200,
            "O nome pode ter no maximo 200 caracteres.");

        // Nome fantasia em pessoa fisica seria dado sem sentido no cadastro.
        ExcecaoDeDominio.SeVerdadeiro(
            TipoDePessoa == TipoDePessoa.Fisica && !string.IsNullOrWhiteSpace(nomeFantasia),
            "Nome fantasia se aplica apenas a pessoa juridica.");

        NomeFantasia = string.IsNullOrWhiteSpace(nomeFantasia) ? null : nomeFantasia.Trim();
        Observacoes = string.IsNullOrWhiteSpace(observacoes) ? null : observacoes.Trim();
    }

    public void DefinirContato(string? email, string? telefone)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            Email = null;
        }
        else
        {
            var normalizado = email.Trim().ToLowerInvariant();

            ExcecaoDeDominio.SeVerdadeiro(
                !EmailTemFormatoValido(normalizado),
                $"O e-mail '{email}' nao e valido.");

            Email = normalizado;
        }

        var digitos = ObjetosDeValor.Documento.SomenteDigitos(telefone);

        if (digitos.Length == 0)
        {
            Telefone = null;
            return;
        }

        // 10 digitos = fixo com DDD; 11 = celular com DDD.
        ExcecaoDeDominio.SeVerdadeiro(
            digitos.Length is not (10 or 11),
            "O telefone precisa ter 10 ou 11 digitos, incluindo o DDD.");

        Telefone = digitos;
    }

    public void DefinirEndereco(Endereco? endereco) => Endereco = endereco;

    public void AlterarDocumento(string documento, TipoDePessoa tipoDePessoa)
    {
        var novo = ObjetosDeValor.Documento.Criar(documento, tipoDePessoa);

        // Trocar de pessoa juridica para fisica tornaria o nome fantasia invalido.
        ExcecaoDeDominio.SeVerdadeiro(
            novo.TipoDePessoa == TipoDePessoa.Fisica && NomeFantasia is not null,
            "Remova o nome fantasia antes de mudar o cadastro para pessoa fisica.");

        Documento = novo.Numero;
        TipoDePessoa = novo.TipoDePessoa;
    }

    /// <summary>Documento com mascara, para exibicao.</summary>
    public string DocumentoFormatado() => ObjetosDeValor.Documento.Formatar(Documento);

    public void Ativar() => Ativo = true;

    public void Inativar() => Ativo = false;

    /// <summary>Telefone formatado para exibicao: (11) 98765-4321.</summary>
    public string? TelefoneFormatado()
    {
        if (Telefone is null)
        {
            return null;
        }

        var ddd = Telefone[..2];
        var restante = Telefone[2..];
        var meio = restante.Length == 9 ? restante[..5] : restante[..4];
        var fim = restante.Length == 9 ? restante[5..] : restante[4..];

        return $"({ddd}) {meio}-{fim}";
    }

    /// <summary>
    /// Validacao deliberadamente simples, igual a usada em <see cref="Usuario"/>:
    /// barra erro grosseiro de digitacao sem tentar implementar a RFC 5322.
    /// </summary>
    private static bool EmailTemFormatoValido(string email)
    {
        var posicaoDoArroba = email.IndexOf('@', StringComparison.Ordinal);

        return posicaoDoArroba > 0
            && posicaoDoArroba < email.Length - 1
            && email.IndexOf('@', posicaoDoArroba + 1) < 0
            && email.Contains('.', StringComparison.Ordinal)
            && !email.Contains(' ', StringComparison.Ordinal);
    }
}
