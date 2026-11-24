using Connectly.Application.Common.Dtos;
using Connectly.Domain.Entities.Notifications.Enums;

namespace Connectly.Application.Notifications.Dtos;

public record NotificationDto(
    Guid Id, NotificationType Type, AuthorDto Actor, Guid? PostId, Guid? GroupId, string? GroupName,
    bool IsRead, DateTime CreatedAtUtc);
