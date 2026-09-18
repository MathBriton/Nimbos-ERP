using Nimbus.Application.Autenticacao.Abstracoes;
using Nimbus.Domain.Entidades;

namespace Nimbus.Tests.Dobras;

/// <summary>Gerador de token previsivel, para asserts diretos nos testes.</summary>
public sealed class GeradorDeTokenFalso : IGeradorDeTokenDeAcesso
{
    public static readonly TimeSpan Validade = TimeSpan.FromHours(1);

    private readonly ProvedorDeDataHoraFalso _relogio;

    public GeradorDeTokenFalso(ProvedorDeDataHoraFalso relogio) => _relogio = relogio;

    public TokenDeAcesso Gerar(Usuario usuario)
    {
        ArgumentNullException.ThrowIfNull(usuario);

        return new TokenDeAcesso($"token-de-{usuario.Email}", _relogio.Agora.Add(Validade));
    }
}
