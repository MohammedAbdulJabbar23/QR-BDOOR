using AlBadour.Application.Common.Interfaces;
using AlBadour.Domain.Entities;
using AlBadour.Domain.Enums;
using AlBadour.Domain.Interfaces;
using AlBadour.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace AlBadour.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepo;
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(
        INotificationRepository notificationRepo,
        IUserRepository userRepo,
        IUnitOfWork unitOfWork,
        IHubContext<NotificationHub> hubContext)
    {
        _notificationRepo = notificationRepo;
        _userRepo = userRepo;
        _unitOfWork = unitOfWork;
        _hubContext = hubContext;
    }

    public async Task SendToUserAsync(Guid userId, string titleAr, string titleEn, string messageAr, string messageEn, string? entityType = null, string? entityId = null, CancellationToken ct = default)
    {
        var notification = new Notification
        {
            RecipientUserId = userId,
            TitleAr = titleAr,
            TitleEn = titleEn,
            MessageAr = messageAr,
            MessageEn = messageEn,
            EntityType = entityType,
            EntityId = entityId
        };

        await _notificationRepo.AddAsync(notification, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await _hubContext.Clients.User(userId.ToString())
            .SendAsync("ReceiveNotification", new
            {
                notification.Id,
                titleAr,
                titleEn,
                messageAr,
                messageEn,
                entityType,
                entityId,
                notification.CreatedAt
            }, ct);
    }

    public async Task SendToDepartmentAsync(Department department, string titleAr, string titleEn, string messageAr, string messageEn, string? entityType = null, string? entityId = null, CancellationToken ct = default)
    {
        var users = await _userRepo.GetByDepartmentAsync(department, ct);
        var notifications = users.Select(u => new Notification
        {
            RecipientUserId = u.Id,
            TitleAr = titleAr,
            TitleEn = titleEn,
            MessageAr = messageAr,
            MessageEn = messageEn,
            EntityType = entityType,
            EntityId = entityId
        }).ToList();

        await _notificationRepo.AddRangeAsync(notifications, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        foreach (var user in users)
        {
            await _hubContext.Clients.User(user.Id.ToString())
                .SendAsync("ReceiveNotification", new
                {
                    titleAr, titleEn, messageAr, messageEn, entityType, entityId,
                    CreatedAt = DateTime.UtcNow
                }, ct);
        }
    }

    public async Task SendToRoleAsync(UserRole role, string titleAr, string titleEn, string messageAr, string messageEn, string? entityType = null, string? entityId = null, CancellationToken ct = default)
    {
        var users = await _userRepo.GetByRoleAsync(role, ct);
        var notifications = users.Select(u => new Notification
        {
            RecipientUserId = u.Id,
            TitleAr = titleAr,
            TitleEn = titleEn,
            MessageAr = messageAr,
            MessageEn = messageEn,
            EntityType = entityType,
            EntityId = entityId
        }).ToList();

        await _notificationRepo.AddRangeAsync(notifications, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        foreach (var user in users)
        {
            await _hubContext.Clients.User(user.Id.ToString())
                .SendAsync("ReceiveNotification", new
                {
                    titleAr, titleEn, messageAr, messageEn, entityType, entityId,
                    CreatedAt = DateTime.UtcNow
                }, ct);
        }
    }
}
