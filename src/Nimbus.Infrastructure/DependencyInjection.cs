using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nimbus.Application.Autenticacao.Abstracoes;
using Nimbus.Application.Common.Abstracoes;
using Nimbus.Domain.Abstracoes;
using Nimbus.Infrastructure.Persistencia;
using Nimbus.Infrastructure.Persistencia.Interceptadores;
using Nimbus.Infrastructure.Persistencia.Repositorios;
using Nimbus.Infrastructure.Seguranca;
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

        servicos.AddSingleton<IProvedorDeDataHora, ProvedorDeDataHoraDoSistema>();

        AdicionarPersistencia(servicos, configuracao);
        AdicionarSeguranca(servicos, configuracao);

        return servicos;
    }

    private static void AdicionarPersistencia(
        IServiceCollection servicos,
        IConfiguration configuracao)
    {
        var conexao = configuracao.GetConnectionString(NomeDaConexao)
            ?? throw new InvalidOperationException(
                $"A connection string '{NomeDaConexao}' nao foi configurada.");

        servicos.AddScoped<InterceptadorDeRastreabilidade>();

        servicos.AddDbContext<NimbusDbContext>((provedor, opcoes) =>
        {
            opcoes.UseSqlServer(conexao, sql =>
            {
                sql.MigrationsAssembly(typeof(NimbusDbContext).Assembly.FullName);

                // Em container o SQL Server pode ficar indisponivel por alguns
                // segundos; a politica de retry evita falha transitoria na subida.
                sql.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
            });

            opcoes.AddInterceptors(provedor.GetRequiredService<InterceptadorDeRastreabilidade>());
        });

        servicos.AddScoped<IRepositorioDeUsuarios, RepositorioDeUsuarios>();

        servicos
            .AddHealthChecks()
            .AddDbContextCheck<NimbusDbContext>(
                name: "sqlserver",
                tags: ["banco", "pronto"]);
    }

    private static void AdicionarSeguranca(
        IServiceCollection servicos,
        IConfiguration configuracao)
    {
        // ValidateOnStart: um segredo ausente ou curto derruba a aplicacao na
        // subida, em vez de so falhar quando alguem tentar fazer login.
        servicos
            .AddOptions<OpcoesDeJwt>()
            .Bind(configuracao.GetSection(OpcoesDeJwt.Secao))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        servicos.AddSingleton<IServicoDeHashDeSenha, ServicoDeHashDeSenhaBCrypt>();
        servicos.AddSingleton<IGeradorDeTokenDeAcesso, GeradorDeTokenJwt>();
    }
}
