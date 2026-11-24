namespace Connectly.Domain.Common;

public abstract class AuditableEntity : MiddleBaseEntity
{
    public DateTime? UpdatedAtUtc { get; set; }
}


