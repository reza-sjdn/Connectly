using Connectly.Application.Common.Dtos;
using Connectly.Domain.Entities.Groups.Enums;

namespace Connectly.Application.Groups.Models;

public record GroupMemberDto(AuthorDto User, GroupRole Role, DateTime JoinedAtUtc);
