using AlBadour.Domain.Entities;
using AlBadour.Domain.Enums;
using AlBadour.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AlBadour.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Username == username, ct);
    }

    public async Task<List<User>> GetByDepartmentAsync(Department department, CancellationToken ct = default)
    {
        return await _context.Users.Where(u => u.Department == department && u.IsActive).ToListAsync(ct);
    }

    public async Task<List<User>> GetByRoleAsync(UserRole role, CancellationToken ct = default)
    {
        return await _context.Users.Where(u => u.Role == role && u.IsActive).ToListAsync(ct);
    }

    public async Task<(List<User> Items, int TotalCount)> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var totalCount = await _context.Users.CountAsync(ct);
        var items = await _context.Users
            .OrderBy(u => u.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await _context.Users.AddAsync(user, ct);
    }

    public void Update(User user)
    {
        _context.Users.Update(user);
    }
}
