using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Nimbus.Api.Common;
using Nimbus.Api.Seguranca;
using Nimbus.Api.Servicos;
using Nimbus.Application;
using Nimbus.Application.Common.Abstracoes;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.Persistencia;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

const string PoliticaDeCorsDoFrontend = "frontend-nimbus";

// ---------------------------------------------------------------------------
// Composicao das camadas (Clean Architecture: API -> Application -> Domain,
// com Infrastructure plugada por inversao de dependencia).
// ---------------------------------------------------------------------------
builder.Services.AdicionarCamadaDeAplicacao();
builder.Services.AdicionarCamadaDeInfraestrutura(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUsuarioAtual, UsuarioAtualHttp>();

builder.Services.AdicionarAutenticacaoJwt(builder.Configuration);

builder.Services
    .AddControllers()
    .AddJsonOptions(opcoes =>
    {
        opcoes.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        opcoes.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        opcoes.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ManipuladorGlobalDeExcecoes>();

builder.Services.AddOpenApi(opcoes =>
    opcoes.AddDocumentTransformer<TransformadorDeSegurancaOpenApi>());

// O frontend roda em outra origem (Vite em dev, Nginx no container).
var origensPermitidas = builder.Configuration
    .GetSection("Cors:OrigensPermitidas")
    .Get<string[]>() ?? [];

builder.Services.AddCors(opcoes => opcoes.AddPolicy(PoliticaDeCorsDoFrontend, politica =>
    politica
        .WithOrigins(origensPermitidas)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
        // Sem expor o header, o navegador o esconde do JavaScript e o frontend
        // nao consegue diferenciar sessao expirada de credencial invalida.
        .WithExposedHeaders(ConfiguracaoDeAutenticacao.HeaderDeTokenExpirado)));

var app = builder.Build();

// ---------------------------------------------------------------------------
// Pipeline HTTP
// ---------------------------------------------------------------------------
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(opcoes => opcoes
        .WithTitle("Nimbus ERP - API")
        .WithTheme(ScalarTheme.BluePlanet));
}

app.UseCors(PoliticaDeCorsDoFrontend);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health checks: /health responde o estado agregado e /health/pronto apenas
// as dependencias criticas (usado pelo healthcheck do Docker Compose).
app.MapHealthChecks("/health", new HealthCheckOptions { ResponseWriter = EscreverRespostaDeSaude });
app.MapHealthChecks("/health/pronto", new HealthCheckOptions
{
    Predicate = registro => registro.Tags.Contains("pronto"),
    ResponseWriter = EscreverRespostaDeSaude,
});

app.MapGet("/", () => Results.Ok(new
{
    aplicacao = "Nimbus ERP",
    versao = "0.1.0",
    ambiente = app.Environment.EnvironmentName,
    documentacao = "/scalar/v1",
    saude = "/health",
}))
.WithName("Raiz")
.WithSummary("Identificacao da API e atalhos uteis.");

if (app.Configuration.GetValue("Banco:AplicarMigrationsNaSubida", defaultValue: true))
{
    await InicializadorDoBanco.AplicarMigrationsAsync(app.Services).ConfigureAwait(false);
}

if (app.Configuration.GetValue("Seed:Habilitado", defaultValue: true))
{
    await SemeadorDeDados.SemearAsync(app.Services).ConfigureAwait(false);
}

await app.RunAsync().ConfigureAwait(false);

// Resposta de health legivel, consumida tambem pela tela de status (Sprint 22).
static Task EscreverRespostaDeSaude(HttpContext contexto, HealthReport relatorio)
{
    contexto.Response.ContentType = "application/json; charset=utf-8";

    var payload = new
    {
        status = relatorio.Status.ToString(),
        duracaoMs = relatorio.TotalDuration.TotalMilliseconds,
        verificacoes = relatorio.Entries.Select(entrada => new
        {
            nome = entrada.Key,
            status = entrada.Value.Status.ToString(),
            duracaoMs = entrada.Value.Duration.TotalMilliseconds,
            descricao = entrada.Value.Description,
        }),
    };

    return contexto.Response.WriteAsJsonAsync(payload);
}
