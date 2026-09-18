namespace Nimbus.Domain.Common;

/// <summary>
/// Raiz comum de todas as entidades persistidas do ERP.
/// Carrega a identidade e os campos de rastreabilidade que o
/// interceptor de auditoria (Sprint 13) vai preencher automaticamente.
/// </summary>
public abstract class EntidadeBase
{
    public Guid Id { get; protected set; } = Guid.CreateVersion7();

    public DateTimeOffset CriadoEm { get; set; }

    public string? CriadoPor { get; set; }

    public DateTimeOffset? AtualizadoEm { get; set; }

    public string? AtualizadoPor { get; set; }

    /// <summary>
    /// Exclusao logica: nenhum registro de negocio e removido fisicamente,
    /// para que a auditoria e os relatorios historicos permanecam consistentes.
    /// </summary>
    public bool Excluido { get; set; }
}
