using Connectly.Application.Common.Dtos;

namespace Connectly.Application.Stories.Dtos;

public record StoryViewerDto(AuthorDto User, DateTime ViewedAtUtc);
