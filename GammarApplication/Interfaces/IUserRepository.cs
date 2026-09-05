using GammarApplication.DTOs.Users;
using GammarDomain.Entities;

namespace GammarApplication.Interfaces;

public interface IUserRepository
{
    Task<List<AdminUserListItemDto>> GetAdminUsersAsync(
        string? keyword,
        string? status,
        string? gender,
        long? profileCharacterId,
        bool? hasPhone,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<User?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    void Update(User user);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
