namespace Nimbus.Application.Common.Excecoes;

/// <summary>
/// Falha de autenticacao. A API traduz para HTTP 401 com ProblemDetails.
/// A mensagem e sempre segura para exibir ao usuario final.
/// </summary>
public class ExcecaoDeAutenticacao : Exception
{
    /// <summary>Mensagem generica usada quando e-mail ou senha nao conferem.</summary>
    public const string CredenciaisInvalidas = "E-mail ou senha invalidos.";

    public ExcecaoDeAutenticacao(string mensagem)
        : base(mensagem)
    {
    }

    public ExcecaoDeAutenticacao(string mensagem, Exception excecaoInterna)
        : base(mensagem, excecaoInterna)
    {
    }
}
