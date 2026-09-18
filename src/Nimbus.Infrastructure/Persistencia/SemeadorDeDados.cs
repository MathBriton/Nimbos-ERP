using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nimbus.Application.Autenticacao.Abstracoes;
using Nimbus.Domain.Abstracoes;
using Nimbus.Domain.Entidades;

namespace Nimbus.Infrastructure.Persistencia;

/// <summary>
/// Cria o usuario administrador inicial, sem o qual ninguem conseguiria entrar
/// no sistema recem-instalado. A operacao e idempotente: se o e-mail ja existe,
/// nada acontece.
/// </summary>
public static class SemeadorDeDados
{
    public const string SecaoDeConfiguracao = "Seed:Administrador";

    private const string NomePadrao = "Administrador";
    private const string EmailPadrao = "admin@erp.com";
    private const string SenhaPadrao = "Admin123!";

    public static async Task SemearAsync(
        IServiceProvider provedorDeServicos,
        CancellationToken cancelamento = default)
    {
        ArgumentNullException.ThrowIfNull(provedorDeServicos);

        await using var escopo = provedorDeServicos.CreateAsyncScope();
        var servicos = escopo.ServiceProvider;

        var logger = servicos
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(typeof(SemeadorDeDados));

        var configuracao = servicos.GetRequiredService<IConfiguration>();
        var repositorio = servicos.GetRequiredService<IRepositorioDeUsuarios>();
        var hash = servicos.GetRequiredService<IServicoDeHashDeSenha>();

        var secao = configuracao.GetSection(SecaoDeConfiguracao);
        var nome = secao["Nome"] ?? NomePadrao;
        var email = secao["Email"] ?? EmailPadrao;
        var senha = secao["Senha"] ?? SenhaPadrao;

        var emailNormalizado = Usuario.NormalizarEmail(email);

        if (await repositorio.ExisteComEmailAsync(emailNormalizado, cancelamento).ConfigureAwait(false))
        {
            logger.LogInformation(
                "Usuario administrador ({Email}) ja existe; seed ignorado.", emailNormalizado);
            return;
        }

        var administrador = Usuario.Criar(nome, emailNormalizado, hash.GerarHash(senha));

        await repositorio.AdicionarAsync(administrador, cancelamento).ConfigureAwait(false);
        await repositorio.SalvarAlteracoesAsync(cancelamento).ConfigureAwait(false);

        logger.LogWarning(
            "Usuario administrador criado: {Email}. Troque a senha padrao antes de expor a API.",
            emailNormalizado);
    }
}
