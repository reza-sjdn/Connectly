using MediatR;

namespace Connectly.Application.Stories.Commands.MarkStoryViewed;

/// <summary>Records that I saw someone's story (idempotent; the author's own views are not counted).</summary>
public record MarkStoryViewedCommand(Guid StoryId) : IRequest;
