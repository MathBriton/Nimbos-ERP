namespace Nimbus.Application.Common.Excecoes;

/// <summary>
/// Conflito com o estado atual dos dados, tipicamente violacao de unicidade.
/// A API traduz para HTTP 409 com ProblemDetails.
///
/// Distinto de <see cref="Nimbus.Domain.Common.ExcecaoDeDominio"/> (400) de
/// proposito: 400 e "o que voce mandou esta errado", 409 e "o que voce mandou
/// esta correto, mas conflita com algo que ja existe".
/// </summary>
public class ExcecaoDeConflito : Exception
{
    public ExcecaoDeConflito(string mensagem)
        : base(mensagem)
    {
    }

    public ExcecaoDeConflito(string mensagem, Exception excecaoInterna)
        : base(mensagem, excecaoInterna)
    {
    }
}
