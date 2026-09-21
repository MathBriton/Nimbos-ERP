using System.Text.RegularExpressions;
using Nimbus.Domain.Common;

namespace Nimbus.Domain.ObjetosDeValor;

/// <summary>
/// Endereco de um cadastro.
///
/// E opcional como um todo: um cliente pode ser cadastrado so com contato. Mas
/// se o usuario comecar a preencher, exigimos o conjunto minimo que torna o
/// endereco utilizavel (CEP, logradouro, numero, bairro, cidade e UF) - meio
/// endereco nao serve nem para entrega nem para nota fiscal.
/// </summary>
public sealed partial record Endereco
{
    /// <summary>Unidades federativas validas, usadas na validacao da UF.</summary>
    public static readonly IReadOnlySet<string> UnidadesFederativas = new HashSet<string>(
        StringComparer.Ordinal)
    {
        "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA", "MT", "MS",
        "MG", "PA", "PB", "PR", "PE", "PI", "RJ", "RN", "RS", "RO", "RR", "SC",
        "SP", "SE", "TO",
    };

    private Endereco(
        string cep,
        string logradouro,
        string numero,
        string? complemento,
        string bairro,
        string cidade,
        string uf)
    {
        Cep = cep;
        Logradouro = logradouro;
        Numero = numero;
        Complemento = complemento;
        Bairro = bairro;
        Cidade = cidade;
        Uf = uf;
    }

    /// <summary>Somente digitos, sem hifen.</summary>
    public string Cep { get; }

    public string Logradouro { get; }

    public string Numero { get; }

    public string? Complemento { get; }

    public string Bairro { get; }

    public string Cidade { get; }

    /// <summary>Sigla da unidade federativa, em maiusculas.</summary>
    public string Uf { get; }

    [GeneratedRegex(@"\D")]
    private static partial Regex ApenasNaoDigitos();

    /// <summary>
    /// Monta o endereco quando ha qualquer campo preenchido; devolve
    /// <c>null</c> quando todos estao vazios (endereco nao informado).
    /// </summary>
    public static Endereco? CriarOuNulo(
        string? cep,
        string? logradouro,
        string? numero,
        string? complemento,
        string? bairro,
        string? cidade,
        string? uf)
    {
        var preenchidos = new[] { cep, logradouro, numero, complemento, bairro, cidade, uf };

        if (preenchidos.All(string.IsNullOrWhiteSpace))
        {
            return null;
        }

        var cepLimpo = ApenasNaoDigitos().Replace(cep ?? string.Empty, string.Empty);

        ExcecaoDeDominio.SeVerdadeiro(
            cepLimpo.Length != 8,
            "O CEP precisa ter 8 digitos.");

        var ufNormalizada = (uf ?? string.Empty).Trim().ToUpperInvariant();

        ExcecaoDeDominio.SeVerdadeiro(
            !UnidadesFederativas.Contains(ufNormalizada),
            $"UF invalida: '{uf}'.");

        return new Endereco(
            cepLimpo,
            ExcecaoDeDominio.TextoObrigatorio(logradouro, "Logradouro"),
            ExcecaoDeDominio.TextoObrigatorio(numero, "Numero"),
            string.IsNullOrWhiteSpace(complemento) ? null : complemento.Trim(),
            ExcecaoDeDominio.TextoObrigatorio(bairro, "Bairro"),
            ExcecaoDeDominio.TextoObrigatorio(cidade, "Cidade"),
            ufNormalizada);
    }

    /// <summary>Reconstroi a partir de valores ja validados (vindos do banco).</summary>
    public static Endereco DoBanco(
        string cep,
        string logradouro,
        string numero,
        string? complemento,
        string bairro,
        string cidade,
        string uf) => new(cep, logradouro, numero, complemento, bairro, cidade, uf);

    public string CepFormatado() => $"{Cep[..5]}-{Cep[5..]}";

    /// <summary>Endereco em uma linha, para listagens e relatorios.</summary>
    public string EmUmaLinha()
    {
        var inicio = string.IsNullOrWhiteSpace(Complemento)
            ? $"{Logradouro}, {Numero}"
            : $"{Logradouro}, {Numero} - {Complemento}";

        return $"{inicio}, {Bairro}, {Cidade}/{Uf}, {CepFormatado()}";
    }
}
