using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nimbus.Application.Common.Abstracoes;
using Nimbus.Infrastructure.Persistencia;
using Nimbus.Infrastructure.Servicos;

namespace Nimbus.Infrastructure;

/// <summary>Registro dos servicos de infraestrutura (persistencia e adaptadores).</summary>
public static class DependencyInjection
{
    public const string NomeDaConexao = "NimbusDb";

    public static IServiceCollection AdicionarCamadaDeInfraestrutura(
        this IServiceCollection servicos,
        IConfiguration configuracao)
    {
        ArgumentNullException.ThrowIfNull(servicos);
        ArgumentNullException.ThrowIfNull(configuracao);

        var conexao = configuracao.GetConnectionString(NomeDaConexao)
            ?? throw new InvalidOperationException(
                $"A connection string '{NomeDaConexao}' nao foi configurada.");

        servicos.AddDbContext<NimbusDbContext>(opcoes =>
            opcoes.UseSqlServer(conexao, sql =>
            {
                sql.MigrationsAssembly(typeof(NimbusDbContext).Assembly.FullName);

                // Em container o SQL Server pode ficar indisponivel por alguns
                // segundos; a politica de retry evita falha transitoria na subida.
                sql.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
            }));

        servicos.AddSingleton<IProvedorDeDataHora, ProvedorDeDataHoraDoSistema>();

        servicos
            .AddHealthChecks()
            .AddDbContextCheck<NimbusDbContext>(
                name: "sqlserver",
                tags: ["banco", "pronto"]);

        return servicos;
    }
}
