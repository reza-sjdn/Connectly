using Connectly.Application.Common.Models;
using Connectly.Domain.Entities;

namespace Connectly.Application.Groups.Models;

public enum GroupMembershipState { None = 0, Pending = 1, Member = 2, Admin = 3 }
public enum GroupListScope { Mine, Discover }

public record GroupSummaryDto(Guid Id, string Name, GroupPrivacy Privacy, int MembersCount, GroupMembershipState MyState);

public record GroupDetailsDto(
    Guid Id, string Name, string? Description, GroupPrivacy Privacy,
    int MembersCount, GroupMembershipState MyState, DateTime CreatedAtUtc);

public record GroupMemberDto(AuthorDto User, GroupRole Role, DateTime JoinedAtUtc);

public record JoinRequestDto(AuthorDto User, DateTime RequestedAtUtc);

public record GroupMessageDto(Guid Id, Guid GroupId, AuthorDto Sender, string Content, DateTime CreatedAtUtc);
