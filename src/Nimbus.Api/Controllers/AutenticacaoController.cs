using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nimbus.Application.Autenticacao;
using Nimbus.Application.Autenticacao.Modelos;
using Nimbus.Application.Common.Abstracoes;
using Nimbus.Application.Common.Excecoes;

namespace Nimbus.Api.Controllers;

/// <summary>Endpoints de autenticacao do ERP.</summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public sealed class AutenticacaoController : ControllerBase
{
    private readonly ServicoDeAutenticacao _servicoDeAutenticacao;
    private readonly IUsuarioAtual _usuarioAtual;

    public AutenticacaoController(
        ServicoDeAutenticacao servicoDeAutenticacao,
        IUsuarioAtual usuarioAtual)
    {
        _servicoDeAutenticacao = servicoDeAutenticacao;
        _usuarioAtual = usuarioAtual;
    }

    /// <summary>Autentica por e-mail e senha e devolve o token de acesso.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType<LoginResposta>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResposta>> Login(
        [FromBody] LoginRequisicao requisicao,
        CancellationToken cancelamento)
    {
        var resposta = await _servicoDeAutenticacao
            .AutenticarAsync(requisicao, cancelamento)
            .ConfigureAwait(false);

        return Ok(resposta);
    }

    /// <summary>
    /// Devolve os dados do usuario do token. O frontend chama este endpoint ao
    /// recarregar a pagina, para reidratar a sessao e descartar token expirado.
    /// </summary>
    [HttpGet("eu")]
    [Authorize]
    [ProducesResponseType<UsuarioAutenticadoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UsuarioAutenticadoDto>> Eu(CancellationToken cancelamento)
    {
        if (_usuarioAtual.Id is not { } id)
        {
            throw new ExcecaoDeAutenticacao("Sessao invalida. Faca login novamente.");
        }

        var usuario = await _servicoDeAutenticacao
            .ObterPorIdAsync(id, cancelamento)
            .ConfigureAwait(false);

        return Ok(usuario);
    }
}
