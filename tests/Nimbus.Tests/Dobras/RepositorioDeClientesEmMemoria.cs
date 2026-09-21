using Nimbus.Domain.Abstracoes;
using Nimbus.Domain.Entidades;

namespace Nimbus.Tests.Dobras;

/// <summary>
/// Repositorio de clientes em memoria.
///
/// Reproduz o contrato da porta - filtros, ordenacao e paginacao - sem banco,
/// para testar as regras do caso de uso. A traducao para SQL do repositorio de
/// verdade (LIKE, indice unico filtrado) e verificada contra a API em execucao,
/// nao aqui: um dublê nunca provaria que a consulta traduz.
/// </summary>
public sealed class RepositorioDeClientesEmMemoria : IRepositorioDeClientes
{
    private readonly List<Cliente> _clientes = [];

    public int QuantidadeDeSalvamentos { get; private set; }

    public IReadOnlyList<Cliente> Todos => _clientes;

    public void Semear(params Cliente[] clientes) => _clientes.AddRange(clientes);

    public Task<(IReadOnlyList<Cliente> Itens, int Total)> ListarAsync(
        FiltroDeClientes filtro,
        CancellationToken cancelamento = default)
    {
        ArgumentNullException.ThrowIfNull(filtro);

        var consulta = _clientes.Where(cliente => !cliente.Excluido);

        if (filtro.Ativo is { } ativo)
        {
            consulta = consulta.Where(cliente => cliente.Ativo == ativo);
        }

        if (filtro.TipoDePessoa is { } tipo)
        {
            consulta = consulta.Where(cliente => cliente.TipoDePessoa == tipo);
        }

        if (!string.IsNullOrWhiteSpace(filtro.Busca))
        {
            var termo = filtro.Busca.Trim();

            consulta = consulta.Where(cliente =>
                cliente.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase)
                || (cliente.NomeFantasia?.Contains(termo, StringComparison.OrdinalIgnoreCase) ?? false)
                || (cliente.Email?.Contains(termo, StringComparison.OrdinalIgnoreCase) ?? false)
                || cliente.Documento.Contains(termo, StringComparison.Ordinal));
        }

        var ordenada = (filtro.Ordenacao, filtro.Descendente) switch
        {
            (OrdenacaoDeClientes.Documento, false) => consulta.OrderBy(c => c.Documento),
            (OrdenacaoDeClientes.Documento, true) => consulta.OrderByDescending(c => c.Documento),
            (OrdenacaoDeClientes.CriadoEm, false) => consulta.OrderBy(c => c.CriadoEm),
            (OrdenacaoDeClientes.CriadoEm, true) => consulta.OrderByDescending(c => c.CriadoEm),
            (_, true) => consulta.OrderByDescending(c => c.Nome),
            _ => consulta.OrderBy(c => c.Nome, StringComparer.Ordinal),
        };

        var todos = ordenada.ToList();

        var pagina = todos
            .Skip(filtro.QuantidadeParaIgnorar)
            .Take(filtro.QuantidadeParaTrazer)
            .ToList();

        return Task.FromResult<(IReadOnlyList<Cliente>, int)>((pagina, todos.Count));
    }

    public Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken cancelamento = default) =>
        Task.FromResult(_clientes.FirstOrDefault(
            cliente => cliente.Id == id && !cliente.Excluido));

    public Task<bool> ExisteComDocumentoAsync(
        string numeroDoDocumento,
        Guid? idParaIgnorar = null,
        CancellationToken cancelamento = default) =>
        Task.FromResult(_clientes.Any(cliente =>
            !cliente.Excluido
            && string.Equals(cliente.Documento, numeroDoDocumento, StringComparison.Ordinal)
            && (idParaIgnorar is null || cliente.Id != idParaIgnorar)));

    public Task AdicionarAsync(Cliente cliente, CancellationToken cancelamento = default)
    {
        _clientes.Add(cliente);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Espelha o comportamento real: o interceptor de rastreabilidade converte
    /// a exclusao em logica, entao o registro continua na colecao.
    /// </summary>
    public void Remover(Cliente cliente)
    {
        ArgumentNullException.ThrowIfNull(cliente);

        cliente.Excluido = true;
    }

    public Task SalvarAlteracoesAsync(CancellationToken cancelamento = default)
    {
        QuantidadeDeSalvamentos++;
        return Task.CompletedTask;
    }
}
