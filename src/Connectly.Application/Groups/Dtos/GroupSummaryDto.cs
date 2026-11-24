using Connectly.Application.Groups.Enums;
using Connectly.Domain.Entities.Groups.Enums;

namespace Connectly.Application.Groups.Models;

public record GroupSummaryDto(Guid Id, string Name, GroupPrivacy Privacy, int MembersCount, GroupMembershipState MyState);
