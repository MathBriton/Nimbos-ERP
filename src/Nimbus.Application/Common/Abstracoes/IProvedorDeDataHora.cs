namespace Nimbus.Application.Common.Abstracoes;

/// <summary>
/// Abstrai o relogio do sistema para manter os casos de uso testaveis
/// (vencimentos do Financeiro, fechamento mensal dos jobs etc.).
/// </summary>
public interface IProvedorDeDataHora
{
    DateTimeOffset Agora { get; }

    DateOnly Hoje { get; }
}
