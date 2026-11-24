using Connectly.Domain.Common;
using Connectly.Domain.Entities.Groups.Enums;

namespace Connectly.Domain.Entities.Groups;

public class Group : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public GroupPrivacy Privacy { get; set; }
    public Guid CreatedById { get; set; }
}
