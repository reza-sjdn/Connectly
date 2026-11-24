using Connectly.Application.Groups.Enums;
using Connectly.Domain.Entities.Groups.Enums;

namespace Connectly.Application.Groups.Models;

public record GroupDetailsDto(
    Guid Id, string Name, string? Description, GroupPrivacy Privacy,
    int MembersCount, GroupMembershipState MyState, DateTime CreatedAtUtc);
