namespace Nimbus.Application.Autenticacao.Modelos;

/// <summary>Resultado de um login bem-sucedido.</summary>
public sealed record LoginResposta
{
    public required string Token { get; init; }

    public required DateTimeOffset ExpiraEm { get; init; }

    public required UsuarioAutenticadoDto Usuario { get; init; }
}
