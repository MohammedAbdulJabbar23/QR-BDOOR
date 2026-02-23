using AlBadour.Domain.Entities;
using AlBadour.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AlBadour.Infrastructure.Persistence.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly ApplicationDbContext _context;

    public NotificationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Notification notification, CancellationToken ct = default)
    {
        await _context.Notifications.AddAsync(notification, ct);
    }

    public async Task AddRangeAsync(IEnumerable<Notification> notifications, CancellationToken ct = default)
    {
        await _context.Notifications.AddRangeAsync(notifications, ct);
    }

    public async Task<(List<Notification> Items, int TotalCount)> GetByRecipientAsync(
        Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _context.Notifications.Where(n => n.RecipientUserId == userId);
        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Notifications.CountAsync(n => n.RecipientUserId == userId && !n.IsRead, ct);
    }

    public async Task<Notification?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id, ct);
    }

    public async Task MarkAllAsReadAsync(Guid userId, CancellationToken ct = default)
    {
        await _context.Notifications
            .Where(n => n.RecipientUserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), ct);
    }
}
