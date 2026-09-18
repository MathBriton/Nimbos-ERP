using Microsoft.Extensions.DependencyInjection;

namespace Nimbus.Application;

/// <summary>
/// Ponto unico de registro dos servicos da camada de aplicacao.
/// Cada sprint que adiciona um caso de uso o registra aqui.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AdicionarCamadaDeAplicacao(this IServiceCollection servicos)
    {
        ArgumentNullException.ThrowIfNull(servicos);

        // Os casos de uso dos modulos de negocio entram a partir da Sprint 1.
        return servicos;
    }
}
