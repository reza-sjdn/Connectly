using Connectly.Application.Common.Dtos;

namespace Connectly.Application.Groups.Models;

public record JoinRequestDto(AuthorDto User, DateTime RequestedAtUtc);
