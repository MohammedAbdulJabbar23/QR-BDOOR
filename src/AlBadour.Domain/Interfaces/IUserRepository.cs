using AlBadour.Domain.Entities;
using AlBadour.Domain.Enums;

namespace AlBadour.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<List<User>> GetByDepartmentAsync(Department department, CancellationToken ct = default);
    Task<List<User>> GetByRoleAsync(UserRole role, CancellationToken ct = default);
    Task<(List<User> Items, int TotalCount)> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    void Update(User user);
}
