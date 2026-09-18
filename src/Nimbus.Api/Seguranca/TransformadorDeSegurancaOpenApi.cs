using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Nimbus.Api.Seguranca;

/// <summary>
/// Declara o esquema Bearer no documento OpenAPI. Sem isso o Scalar nao oferece
/// o campo de token e nao da para exercitar os endpoints protegidos pela UI.
/// </summary>
public sealed class TransformadorDeSegurancaOpenApi : IOpenApiDocumentTransformer
{
    private const string NomeDoEsquema = "Bearer";

    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(document);

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        document.Components.SecuritySchemes[NomeDoEsquema] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Informe apenas o token devolvido por POST /api/auth/login.",
        };

        return Task.CompletedTask;
    }
}
