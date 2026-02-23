namespace AlBadour.Application.Features.Notifications.DTOs;

public record NotificationDto(
    long Id,
    string TitleAr,
    string TitleEn,
    string MessageAr,
    string MessageEn,
    string? EntityType,
    string? EntityId,
    bool IsRead,
    DateTime CreatedAt
);
