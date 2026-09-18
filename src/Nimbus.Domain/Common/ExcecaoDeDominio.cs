namespace Nimbus.Domain.Common;

/// <summary>
/// Sinaliza a violacao de uma invariante de negocio (ex.: saldo de estoque
/// negativo). A API traduz essa excecao para HTTP 400 com ProblemDetails.
/// </summary>
public class ExcecaoDeDominio : Exception
{
    public ExcecaoDeDominio(string mensagem)
        : base(mensagem)
    {
    }

    public ExcecaoDeDominio(string mensagem, Exception excecaoInterna)
        : base(mensagem, excecaoInterna)
    {
    }

    /// <summary>Lanca <see cref="ExcecaoDeDominio"/> quando <paramref name="condicao"/> for verdadeira.</summary>
    public static void SeVerdadeiro(bool condicao, string mensagem)
    {
        if (condicao)
        {
            throw new ExcecaoDeDominio(mensagem);
        }
    }

    /// <summary>Lanca <see cref="ExcecaoDeDominio"/> quando o texto estiver vazio ou em branco.</summary>
    public static string TextoObrigatorio(string? valor, string nomeDoCampo)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new ExcecaoDeDominio($"O campo '{nomeDoCampo}' e obrigatorio.");
        }

        return valor.Trim();
    }
}
