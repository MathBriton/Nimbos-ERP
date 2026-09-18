using Nimbus.Application.Common.Abstracoes;

namespace Nimbus.Tests.Dobras;

/// <summary>Relogio controlavel, para testar vencimentos e bloqueios sem esperar.</summary>
public sealed class ProvedorDeDataHoraFalso : IProvedorDeDataHora
{
    public ProvedorDeDataHoraFalso(DateTimeOffset? inicio = null) =>
        Agora = inicio ?? new DateTimeOffset(2026, 1, 15, 10, 0, 0, TimeSpan.Zero);

    public DateTimeOffset Agora { get; set; }

    public DateOnly Hoje => DateOnly.FromDateTime(Agora.UtcDateTime);

    public void Avancar(TimeSpan intervalo) => Agora = Agora.Add(intervalo);
}
