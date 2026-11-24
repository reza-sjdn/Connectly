using Connectly.Application.Stories.Dtos;
using MediatR;

namespace Connectly.Application.Stories.Queries.GetStoryFeed;

/// <summary>Active stories of me + people I follow, grouped per author: mine first, then unseen, then newest.</summary>
public record GetStoryFeedQuery : IRequest<IReadOnlyList<StoryGroupDto>>;
