using System.Text.RegularExpressions;
using Nimbus.Domain.Common;

namespace Nimbus.Domain.ObjetosDeValor;

/// <summary>Natureza do cadastro, que determina o tipo de documento exigido.</summary>
public enum TipoDePessoa
{
    Fisica = 1,
    Juridica = 2,
}

/// <summary>
/// CPF ou CNPJ ja validado.
///
/// Guardado sempre sem mascara (somente digitos): a formatacao e assunto de
/// apresentacao, e persistir sem ela evita que o mesmo documento entre duas
/// vezes com pontuacoes diferentes.
///
/// Nota: a Receita Federal introduziu o CNPJ alfanumerico em 2026. Esta
/// implementacao cobre o formato numerico, que segue valido e e o unico em uso
/// na base atual; o algoritmo de digito verificador e o mesmo, entao suportar
/// o alfanumerico depois e uma extensao localizada nesta classe.
/// </summary>
public sealed partial record Documento
{
    public const int TamanhoDoCpf = 11;
    public const int TamanhoDoCnpj = 14;

    private Documento(string numero) => Numero = numero;

    /// <summary>Somente digitos, sem pontuacao.</summary>
    public string Numero { get; }

    /// <summary>
    /// Derivado do tamanho, e nao guardado: 11 digitos so pode ser CPF e 14 so
    /// pode ser CNPJ. Derivar mantem o objeto com um unico campo, o que permite
    /// persisti-lo em uma coluna so - e, por tabela, indexa-lo e consulta-lo.
    /// </summary>
    public TipoDePessoa TipoDePessoa =>
        Numero.Length == TamanhoDoCpf ? TipoDePessoa.Fisica : TipoDePessoa.Juridica;

    [GeneratedRegex(@"\D")]
    private static partial Regex ApenasNaoDigitos();

    /// <summary>Remove qualquer caractere que nao seja digito.</summary>
    public static string SomenteDigitos(string? valor) =>
        string.IsNullOrEmpty(valor) ? string.Empty : ApenasNaoDigitos().Replace(valor, string.Empty);

    /// <summary>
    /// Cria o documento validando o formato e os digitos verificadores,
    /// conforme o tipo de pessoa informado.
    /// </summary>
    /// <exception cref="ExcecaoDeDominio">Documento invalido para o tipo informado.</exception>
    public static Documento Criar(string? valor, TipoDePessoa tipoDePessoa)
    {
        var digitos = SomenteDigitos(valor);

        ExcecaoDeDominio.SeVerdadeiro(
            digitos.Length == 0,
            tipoDePessoa == TipoDePessoa.Fisica ? "Informe o CPF." : "Informe o CNPJ.");

        return tipoDePessoa switch
        {
            TipoDePessoa.Fisica => CriarCpf(digitos),
            TipoDePessoa.Juridica => CriarCnpj(digitos),
            _ => throw new ExcecaoDeDominio("Tipo de pessoa invalido."),
        };
    }

    /// <summary>Valida sem lancar excecao. Util em filtros e buscas.</summary>
    public static bool EhValido(string? valor, TipoDePessoa tipoDePessoa)
    {
        var digitos = SomenteDigitos(valor);

        return tipoDePessoa switch
        {
            TipoDePessoa.Fisica => CpfEhValido(digitos),
            TipoDePessoa.Juridica => CnpjEhValido(digitos),
            _ => false,
        };
    }

    /// <summary>
    /// Reconstroi a partir de um valor ja validado (vindo do banco), sem
    /// repetir a validacao a cada leitura.
    /// </summary>
    public static Documento DoBanco(string numero) => new(numero);

    private static Documento CriarCpf(string digitos)
    {
        ExcecaoDeDominio.SeVerdadeiro(
            digitos.Length != TamanhoDoCpf,
            "O CPF precisa ter 11 digitos.");

        ExcecaoDeDominio.SeVerdadeiro(!CpfEhValido(digitos), "CPF invalido.");

        return new Documento(digitos);
    }

    private static Documento CriarCnpj(string digitos)
    {
        ExcecaoDeDominio.SeVerdadeiro(
            digitos.Length != TamanhoDoCnpj,
            "O CNPJ precisa ter 14 digitos.");

        ExcecaoDeDominio.SeVerdadeiro(!CnpjEhValido(digitos), "CNPJ invalido.");

        return new Documento(digitos);
    }

    private static bool CpfEhValido(string digitos)
    {
        if (digitos.Length != TamanhoDoCpf || TodosOsDigitosIguais(digitos))
        {
            // 111.111.111-11 e afins passam no calculo do DV, mas a Receita
            // Federal nao os emite - por isso a rejeicao explicita.
            return false;
        }

        var primeiro = DigitoVerificador(digitos, quantidade: 9, pesoInicial: 10);
        var segundo = DigitoVerificador(digitos, quantidade: 10, pesoInicial: 11);

        return digitos[9] == primeiro && digitos[10] == segundo;
    }

    private static bool CnpjEhValido(string digitos)
    {
        if (digitos.Length != TamanhoDoCnpj || TodosOsDigitosIguais(digitos))
        {
            return false;
        }

        var primeiro = DigitoVerificadorDoCnpj(digitos, quantidade: 12);
        var segundo = DigitoVerificadorDoCnpj(digitos, quantidade: 13);

        return digitos[12] == primeiro && digitos[13] == segundo;
    }

    private static bool TodosOsDigitosIguais(string digitos) =>
        digitos.All(digito => digito == digitos[0]);

    /// <summary>
    /// Digito verificador do CPF: pesos decrescentes a partir de
    /// <paramref name="pesoInicial"/>.
    /// </summary>
    private static char DigitoVerificador(string digitos, int quantidade, int pesoInicial)
    {
        var soma = 0;

        for (var indice = 0; indice < quantidade; indice++)
        {
            soma += (digitos[indice] - '0') * (pesoInicial - indice);
        }

        var resto = soma % 11;

        return resto < 2 ? '0' : (char)('0' + (11 - resto));
    }

    /// <summary>
    /// Digito verificador do CNPJ: pesos ciclicos de 2 a 9, aplicados da
    /// direita para a esquerda.
    /// </summary>
    private static char DigitoVerificadorDoCnpj(string digitos, int quantidade)
    {
        var soma = 0;
        var peso = 2;

        for (var indice = quantidade - 1; indice >= 0; indice--)
        {
            soma += (digitos[indice] - '0') * peso;
            peso = peso == 9 ? 2 : peso + 1;
        }

        var resto = soma % 11;

        return resto < 2 ? '0' : (char)('0' + (11 - resto));
    }

    /// <summary>Devolve o documento formatado para exibicao.</summary>
    public string Formatado() => Formatar(Numero);

    /// <summary>
    /// Formata um numero ja normalizado. Versao estatica para quem guarda
    /// apenas os digitos e nao quer reconstruir o objeto de valor so para exibir.
    /// </summary>
    public static string Formatar(string numero)
    {
        ArgumentException.ThrowIfNullOrEmpty(numero);

        return numero.Length == TamanhoDoCpf
            ? $"{numero[..3]}.{numero[3..6]}.{numero[6..9]}-{numero[9..]}"
            : $"{numero[..2]}.{numero[2..5]}.{numero[5..8]}/{numero[8..12]}-{numero[12..]}";
    }

    public override string ToString() => Formatado();
}
