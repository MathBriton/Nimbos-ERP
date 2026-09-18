using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Nimbus.Application.Common.Excecoes;
using Nimbus.Domain.Common;

namespace Nimbus.Api.Common;

/// <summary>
/// Converte excecoes nao tratadas em respostas ProblemDetails (RFC 9457).
/// Violacoes de invariante de negocio viram 400; o resto vira 500 sem
/// vazar stack trace para o cliente.
/// </summary>
public sealed class ManipuladorGlobalDeExcecoes : IExceptionHandler
{
    private readonly IProblemDetailsService _servicoDeProblemDetails;
    private readonly ILogger<ManipuladorGlobalDeExcecoes> _logger;

    public ManipuladorGlobalDeExcecoes(
        IProblemDetailsService servicoDeProblemDetails,
        ILogger<ManipuladorGlobalDeExcecoes> logger)
    {
        _servicoDeProblemDetails = servicoDeProblemDetails;
        _logger = logger;
    }

    // Os nomes dos parametros acompanham a assinatura de IExceptionHandler.
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var (status, titulo, detalhe) = exception switch
        {
            ExcecaoDeAutenticacao autenticacao => (
                StatusCodes.Status401Unauthorized,
                "Falha na autenticacao",
                autenticacao.Message),
            ExcecaoDeDominio dominio => (
                StatusCodes.Status400BadRequest,
                "Regra de negocio violada",
                dominio.Message),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Erro interno do servidor",
                "Ocorreu um erro inesperado ao processar a requisicao."),
        };

        if (status == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Erro nao tratado em {Metodo} {Caminho}.",
                httpContext.Request.Method, httpContext.Request.Path);
        }
        else
        {
            _logger.LogWarning("Requisicao recusada em {Metodo} {Caminho}: {Mensagem}",
                httpContext.Request.Method, httpContext.Request.Path, exception?.Message);
        }

        httpContext.Response.StatusCode = status;

        return await _servicoDeProblemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = titulo,
                Detail = detalhe,
                Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}",
            },
        }).ConfigureAwait(false);
    }
}
