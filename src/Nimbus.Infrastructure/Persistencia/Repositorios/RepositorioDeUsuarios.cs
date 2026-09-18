using Microsoft.EntityFrameworkCore;
using Nimbus.Domain.Abstracoes;
using Nimbus.Domain.Entidades;

namespace Nimbus.Infrastructure.Persistencia.Repositorios;

/// <summary>Implementacao de <see cref="IRepositorioDeUsuarios"/> sobre o EF Core.</summary>
public sealed class RepositorioDeUsuarios : IRepositorioDeUsuarios
{
    private readonly NimbusDbContext _contexto;

    public RepositorioDeUsuarios(NimbusDbContext contexto) => _contexto = contexto;

    public Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancelamento = default)
    {
        var emailNormalizado = email?.Trim().ToLowerInvariant() ?? string.Empty;

        return _contexto.Usuarios
            .FirstOrDefaultAsync(usuario => usuario.Email == emailNormalizado, cancelamento);
    }

    public Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken cancelamento = default) =>
        _contexto.Usuarios.FirstOrDefaultAsync(usuario => usuario.Id == id, cancelamento);

    public Task<bool> ExisteComEmailAsync(string email, CancellationToken cancelamento = default)
    {
        var emailNormalizado = email?.Trim().ToLowerInvariant() ?? string.Empty;

        return _contexto.Usuarios
            .AnyAsync(usuario => usuario.Email == emailNormalizado, cancelamento);
    }

    public async Task AdicionarAsync(Usuario usuario, CancellationToken cancelamento = default) =>
        await _contexto.Usuarios.AddAsync(usuario, cancelamento).ConfigureAwait(false);

    public Task SalvarAlteracoesAsync(CancellationToken cancelamento = default) =>
        _contexto.SaveChangesAsync(cancelamento);
}
