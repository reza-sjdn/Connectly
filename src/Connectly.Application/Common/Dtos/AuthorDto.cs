namespace Connectly.Application.Common.Dtos;

public record AuthorDto(Guid Id, string FirstName, string LastName, string? AvatarUrl);
