using Nimbus.Application.Common.Abstracoes;

namespace Nimbus.Infrastructure.Servicos;

/// <summary>Implementacao padrao de <see cref="IProvedorDeDataHora"/> baseada no relogio do host.</summary>
public sealed class ProvedorDeDataHoraDoSistema : IProvedorDeDataHora
{
    public DateTimeOffset Agora => DateTimeOffset.UtcNow;

    public DateOnly Hoje => DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);
}
