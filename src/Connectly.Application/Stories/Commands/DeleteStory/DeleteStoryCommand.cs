using MediatR;

namespace Connectly.Application.Stories.Commands.DeleteStory;

public record DeleteStoryCommand(Guid StoryId) : IRequest;
