using Connectly.Application.Common.Dtos;

namespace Connectly.Application.Stories.Dtos;

public record StoryGroupDto(AuthorDto Author, bool IsMe, bool HasUnseen, IReadOnlyList<StoryDto> Stories);
