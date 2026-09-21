namespace Nimbus.Application.Common.Excecoes;

/// <summary>
/// Recurso inexistente (ou ja excluido logicamente).
/// A API traduz para HTTP 404 com ProblemDetails.
/// </summary>
public class ExcecaoDeRecursoNaoEncontrado : Exception
{
    public ExcecaoDeRecursoNaoEncontrado(string mensagem)
        : base(mensagem)
    {
    }

    public ExcecaoDeRecursoNaoEncontrado(string mensagem, Exception excecaoInterna)
        : base(mensagem, excecaoInterna)
    {
    }

    /// <summary>Mensagem padronizada: "Cliente 123 nao encontrado."</summary>
    public static ExcecaoDeRecursoNaoEncontrado Para(string recurso, object identificador) =>
        new($"{recurso} '{identificador}' nao encontrado.");
}
