using System.Security.Claims;
using Nimbus.Application.Autenticacao;
using Nimbus.Application.Common.Abstracoes;

namespace Nimbus.Api.Servicos;

/// <summary>
/// Le o usuario autenticado a partir das claims do JWT da requisicao corrente.
/// As claims passam a ser efetivamente preenchidas na Sprint 1 (autenticacao).
/// </summary>
public sealed class UsuarioAtualHttp : IUsuarioAtual
{
    private readonly IHttpContextAccessor _acessorDeContexto;

    public UsuarioAtualHttp(IHttpContextAccessor acessorDeContexto) =>
        _acessorDeContexto = acessorDeContexto;

    private ClaimsPrincipal? Principal => _acessorDeContexto.HttpContext?.User;

    public Guid? Id =>
        Guid.TryParse(Principal?.FindFirstValue(ClaimsDoNimbus.Id), out var id) ? id : null;

    public string? Email => Principal?.FindFirstValue(ClaimsDoNimbus.Email);

    public bool EstaAutenticado => Principal?.Identity?.IsAuthenticated ?? false;
}
