using Nimbus.Domain.Abstracoes;
using Nimbus.Domain.Entidades;

namespace Nimbus.Tests.Dobras;

/// <summary>
/// Repositorio em memoria. Evita depender de banco nos testes dos casos de uso
/// e registra quantas vezes as alteracoes foram persistidas, para que os testes
/// possam afirmar que o contador de falhas realmente foi gravado.
/// </summary>
public sealed class RepositorioDeUsuariosEmMemoria : IRepositorioDeUsuarios
{
    private readonly List<Usuario> _usuarios = [];

    public int QuantidadeDeSalvamentos { get; private set; }

    public void Semear(params Usuario[] usuarios) => _usuarios.AddRange(usuarios);

    public Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancelamento = default)
    {
        var normalizado = email?.Trim().ToLowerInvariant();

        return Task.FromResult(
            _usuarios.FirstOrDefault(usuario =>
                string.Equals(usuario.Email, normalizado, StringComparison.Ordinal)));
    }

    public Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken cancelamento = default) =>
        Task.FromResult(_usuarios.FirstOrDefault(usuario => usuario.Id == id));

    public Task<bool> ExisteComEmailAsync(string email, CancellationToken cancelamento = default)
    {
        var normalizado = email?.Trim().ToLowerInvariant();

        return Task.FromResult(
            _usuarios.Any(usuario =>
                string.Equals(usuario.Email, normalizado, StringComparison.Ordinal)));
    }

    public Task AdicionarAsync(Usuario usuario, CancellationToken cancelamento = default)
    {
        _usuarios.Add(usuario);
        return Task.CompletedTask;
    }

    public Task SalvarAlteracoesAsync(CancellationToken cancelamento = default)
    {
        QuantidadeDeSalvamentos++;
        return Task.CompletedTask;
    }
}
