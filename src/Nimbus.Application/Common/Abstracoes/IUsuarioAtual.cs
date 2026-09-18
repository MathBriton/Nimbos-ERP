namespace Nimbus.Application.Common.Abstracoes;

/// <summary>
/// Expoe o usuario autenticado da requisicao corrente para as camadas
/// internas, sem acoplar Application/Infrastructure ao ASP.NET Core.
/// Consumido pela auditoria (Sprint 13) e pelo RBAC (Sprint 15).
/// </summary>
public interface IUsuarioAtual
{
    Guid? Id { get; }

    string? Email { get; }

    bool EstaAutenticado { get; }
}
