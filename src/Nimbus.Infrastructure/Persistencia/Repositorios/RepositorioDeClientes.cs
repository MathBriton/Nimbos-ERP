using Microsoft.EntityFrameworkCore;
using Nimbus.Domain.Abstracoes;
using Nimbus.Domain.Entidades;
using Nimbus.Domain.ObjetosDeValor;

namespace Nimbus.Infrastructure.Persistencia.Repositorios;

/// <summary>Implementacao de <see cref="IRepositorioDeClientes"/> sobre o EF Core.</summary>
public sealed class RepositorioDeClientes : IRepositorioDeClientes
{
    private readonly NimbusDbContext _contexto;

    public RepositorioDeClientes(NimbusDbContext contexto) => _contexto = contexto;

    public async Task<(IReadOnlyList<Cliente> Itens, int Total)> ListarAsync(
        FiltroDeClientes filtro,
        CancellationToken cancelamento = default)
    {
        ArgumentNullException.ThrowIfNull(filtro);

        var consulta = _contexto.Clientes.AsNoTracking();

        consulta = AplicarFiltros(consulta, filtro);

        // O total e contado sobre a consulta ja filtrada, mas antes da
        // paginacao: e o numero de registros que casam com a busca, nao o
        // tamanho da pagina.
        var total = await consulta.CountAsync(cancelamento).ConfigureAwait(false);

        var itens = await Ordenar(consulta, filtro)
            .Skip(filtro.QuantidadeParaIgnorar)
            .Take(filtro.QuantidadeParaTrazer)
            .ToListAsync(cancelamento)
            .ConfigureAwait(false);

        return (itens, total);
    }

    public Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken cancelamento = default) =>
        _contexto.Clientes.FirstOrDefaultAsync(cliente => cliente.Id == id, cancelamento);

    public Task<bool> ExisteComDocumentoAsync(
        string numeroDoDocumento,
        Guid? idParaIgnorar = null,
        CancellationToken cancelamento = default)
    {
        return _contexto.Clientes
            .AsNoTracking()
            .AnyAsync(
                cliente => cliente.Documento == numeroDoDocumento
                    && (idParaIgnorar == null || cliente.Id != idParaIgnorar),
                cancelamento);
    }

    public async Task AdicionarAsync(Cliente cliente, CancellationToken cancelamento = default) =>
        await _contexto.Clientes.AddAsync(cliente, cancelamento).ConfigureAwait(false);

    public void Remover(Cliente cliente) => _contexto.Clientes.Remove(cliente);

    public Task SalvarAlteracoesAsync(CancellationToken cancelamento = default) =>
        _contexto.SaveChangesAsync(cancelamento);

    private static IQueryable<Cliente> AplicarFiltros(
        IQueryable<Cliente> consulta,
        FiltroDeClientes filtro)
    {
        if (filtro.Ativo is { } ativo)
        {
            consulta = consulta.Where(cliente => cliente.Ativo == ativo);
        }

        if (filtro.TipoDePessoa is { } tipo)
        {
            consulta = consulta.Where(cliente => cliente.TipoDePessoa == tipo);
        }

        if (string.IsNullOrWhiteSpace(filtro.Busca))
        {
            return consulta;
        }

        var termo = filtro.Busca.Trim();

        // Quando o termo tem digitos, o usuario provavelmente esta procurando
        // pelo documento - e ele esta guardado sem mascara, entao a busca
        // precisa comparar tambem a versao so com digitos do que foi digitado.
        var digitos = Documento.SomenteDigitos(termo);
        var buscarPorDocumento = digitos.Length >= 3;

        return consulta.Where(cliente =>
            EF.Functions.Like(cliente.Nome, $"%{termo}%")
            || (cliente.NomeFantasia != null && EF.Functions.Like(cliente.NomeFantasia, $"%{termo}%"))
            || (cliente.Email != null && EF.Functions.Like(cliente.Email, $"%{termo}%"))
            || (buscarPorDocumento && EF.Functions.Like(cliente.Documento, $"%{digitos}%")));
    }

    private static IQueryable<Cliente> Ordenar(
        IQueryable<Cliente> consulta,
        FiltroDeClientes filtro)
    {
        // O switch sobre enum mantem a ordenacao restrita a colunas conhecidas.
        return (filtro.Ordenacao, filtro.Descendente) switch
        {
            (OrdenacaoDeClientes.Documento, false) => consulta.OrderBy(c => c.Documento),
            (OrdenacaoDeClientes.Documento, true) => consulta.OrderByDescending(c => c.Documento),
            (OrdenacaoDeClientes.CriadoEm, false) => consulta.OrderBy(c => c.CriadoEm),
            (OrdenacaoDeClientes.CriadoEm, true) => consulta.OrderByDescending(c => c.CriadoEm),
            (_, true) => consulta.OrderByDescending(c => c.Nome).ThenByDescending(c => c.Id),
            _ => consulta.OrderBy(c => c.Nome).ThenBy(c => c.Id),
        };
    }
}
