using Microsoft.Extensions.DependencyInjection;
using Nimbus.Application.Autenticacao;

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

        servicos.AddScoped<ServicoDeAutenticacao>();

        return servicos;
    }
}
