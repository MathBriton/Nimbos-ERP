using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Nimbus.Infrastructure.Persistencia;

/// <summary>
/// Aplica as migrations pendentes na subida da aplicacao.
/// Em <c>docker compose up</c> isso torna o ambiente utilizavel sem
/// nenhum passo manual; em producao o ideal e rodar as migrations no pipeline.
/// </summary>
public static class InicializadorDoBanco
{
    public static async Task AplicarMigrationsAsync(
        IServiceProvider provedorDeServicos,
        CancellationToken cancelamento = default)
    {
        ArgumentNullException.ThrowIfNull(provedorDeServicos);

        await using var escopo = provedorDeServicos.CreateAsyncScope();

        var logger = escopo.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(typeof(InicializadorDoBanco));

        var contexto = escopo.ServiceProvider.GetRequiredService<NimbusDbContext>();

        try
        {
            logger.LogInformation("Aplicando migrations pendentes do banco Nimbus...");
            await contexto.Database.MigrateAsync(cancelamento).ConfigureAwait(false);
            logger.LogInformation("Banco Nimbus atualizado com sucesso.");
        }
        catch (Exception excecao)
        {
            logger.LogError(excecao, "Falha ao aplicar as migrations do banco Nimbus.");
            throw;
        }
    }
}
