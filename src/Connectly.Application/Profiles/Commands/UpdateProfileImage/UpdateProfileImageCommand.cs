using Connectly.Application.Common.Dtos;
using Connectly.Application.Profiles.Enums;
using MediatR;

namespace Connectly.Application.Profiles.Commands.UpdateProfileImage;

/// <summary>Returns the new public URL of the image.</summary>
public record UpdateProfileImageCommand(ProfileImageKind Kind, FileUpload File) : IRequest<string>;
