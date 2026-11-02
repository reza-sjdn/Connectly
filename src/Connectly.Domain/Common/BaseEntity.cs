namespace Connectly.Domain.Common;

public interface IHasCreatedAt
{
    DateTime CreatedAtUtc { get; set; }
}

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
}

public abstract class AuditableEntity : BaseEntity, IHasCreatedAt
{
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
