using Connectly.Application.Stories.Dtos;
using MediatR;

namespace Connectly.Application.Stories.Queries.GetStoryViewers;

/// <summary>"Seen by" list - author only.</summary>
public record GetStoryViewersQuery(Guid StoryId) : IRequest<IReadOnlyList<StoryViewerDto>>;
