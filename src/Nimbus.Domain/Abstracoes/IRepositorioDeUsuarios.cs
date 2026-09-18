using Nimbus.Domain.Entidades;

namespace Nimbus.Domain.Abstracoes;

/// <summary>
/// Porta de acesso aos usuarios. A implementacao com EF Core vive em
/// Nimbus.Infrastructure, de modo que o Domain permaneca livre de persistencia.
/// </summary>
public interface IRepositorioDeUsuarios
{
    Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancelamento = default);

    Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken cancelamento = default);

    Task<bool> ExisteComEmailAsync(string email, CancellationToken cancelamento = default);

    Task AdicionarAsync(Usuario usuario, CancellationToken cancelamento = default);

    /// <summary>Persiste as alteracoes pendentes da unidade de trabalho.</summary>
    Task SalvarAlteracoesAsync(CancellationToken cancelamento = default);
}
