using Connectly.Application.Common.Dtos;

namespace Connectly.Application.Groups.Models;

public record GroupMessageDto(Guid Id, Guid GroupId, AuthorDto Sender, string Content, DateTime CreatedAtUtc);
