using Connectly.Application.Common.Dtos;
using Connectly.Application.Stories.Dtos;
using MediatR;

namespace Connectly.Application.Stories.Commands.CreateStory;

/// <summary>A story is EITHER a photo OR text on a colored background, and disappears after 24 hours.</summary>
public record CreateStoryCommand(string? Text, string? BackgroundColor, FileUpload? Image) : IRequest<StoryDto>;
