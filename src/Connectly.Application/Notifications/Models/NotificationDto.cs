using Connectly.Application.Common.Models;
using Connectly.Domain.Entities;

namespace Connectly.Application.Notifications.Models;

public record NotificationDto(
    Guid Id, NotificationType Type, AuthorDto Actor, Guid? PostId, Guid? GroupId, string? GroupName,
    bool IsRead, DateTime CreatedAtUtc);
