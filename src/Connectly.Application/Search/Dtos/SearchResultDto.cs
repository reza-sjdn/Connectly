using Connectly.Application.Common.Dtos;
using Connectly.Application.Groups.Models;
using Connectly.Application.Posts.Dtos;

namespace Connectly.Application.Search.Dtos;

public record SearchResultDto(
    IReadOnlyList<UserSummaryDto> People, IReadOnlyList<GroupSummaryDto> Groups, IReadOnlyList<PostDto> Posts);
