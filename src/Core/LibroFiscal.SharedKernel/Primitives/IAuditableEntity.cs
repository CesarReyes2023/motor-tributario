namespace LibroFiscal.SharedKernel.Primitives;

public interface IAuditableEntity
{
    void SetCreatedAudit(DateTimeOffset timestamp, string userId);
    void SetModifiedAudit(DateTimeOffset timestamp, string userId);
}
