namespace Nimbus.Application.Autenticacao.Modelos;

/// <summary>
/// Dados do usuario devolvidos ao frontend depois do login.
/// Nunca inclui o hash da senha.
/// </summary>
public sealed record UsuarioAutenticadoDto
{
    public required Guid Id { get; init; }

    public required string Nome { get; init; }

    public required string Email { get; init; }
}
