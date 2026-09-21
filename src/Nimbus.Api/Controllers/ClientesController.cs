using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nimbus.Application.Clientes;
using Nimbus.Application.Clientes.Modelos;
using Nimbus.Application.Common.Modelos;

namespace Nimbus.Api.Controllers;

/// <summary>
/// CRUD de clientes.
///
/// Este controller e o modelo que os demais cadastros seguem: listagem
/// paginada com filtro, obter por id, criar, atualizar e excluir.
/// </summary>
[ApiController]
[Route("api/clientes")]
[Authorize]
[Produces("application/json")]
public sealed class ClientesController : ControllerBase
{
    private readonly ServicoDeClientes _servico;

    public ClientesController(ServicoDeClientes servico) => _servico = servico;

    /// <summary>Lista clientes de forma paginada, com busca e filtros.</summary>
    [HttpGet]
    [ProducesResponseType<ResultadoPaginado<ClienteDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ResultadoPaginado<ClienteDto>>> Listar(
        [FromQuery] ConsultaDeClientes consulta,
        CancellationToken cancelamento)
    {
        var resultado = await _servico.ListarAsync(consulta, cancelamento).ConfigureAwait(false);

        return Ok(resultado);
    }

    /// <summary>Obtem um cliente pelo identificador.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ClienteDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteDto>> ObterPorId(
        Guid id,
        CancellationToken cancelamento)
    {
        var cliente = await _servico.ObterPorIdAsync(id, cancelamento).ConfigureAwait(false);

        return Ok(cliente);
    }

    /// <summary>Cadastra um novo cliente.</summary>
    [HttpPost]
    [ProducesResponseType<ClienteDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClienteDto>> Criar(
        [FromBody] SalvarClienteRequisicao requisicao,
        CancellationToken cancelamento)
    {
        var cliente = await _servico.CriarAsync(requisicao, cancelamento).ConfigureAwait(false);

        return CreatedAtAction(nameof(ObterPorId), new { id = cliente.Id }, cliente);
    }

    /// <summary>Atualiza um cliente existente.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<ClienteDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClienteDto>> Atualizar(
        Guid id,
        [FromBody] SalvarClienteRequisicao requisicao,
        CancellationToken cancelamento)
    {
        var cliente = await _servico
            .AtualizarAsync(id, requisicao, cancelamento)
            .ConfigureAwait(false);

        return Ok(cliente);
    }

    /// <summary>
    /// Exclui um cliente. A exclusao e logica: o registro sai das listagens mas
    /// permanece no banco, preservando o historico e a auditoria.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken cancelamento)
    {
        await _servico.ExcluirAsync(id, cancelamento).ConfigureAwait(false);

        return NoContent();
    }
}
