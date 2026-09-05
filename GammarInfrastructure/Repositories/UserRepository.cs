using GammarApplication.DTOs.Users;
using GammarApplication.Interfaces;
using GammarDomain.Entities;
using GammarInfrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GammarInfrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context)
    {
    }

    public Task<List<AdminUserListItemDto>> GetAdminUsersAsync(
        string? keyword,
        string? status,
        string? gender,
        long? profileCharacterId,
        bool? hasPhone,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query =
            from user in _context.Users
            join profile in _context.Profiles on user.Id equals profile.UserId into profileGroup
            from profile in profileGroup.DefaultIfEmpty()
            join character in _context.ProfileCharacters on profile.ProfileCharacterId equals character.Id into characterGroup
            from character in characterGroup.DefaultIfEmpty()
            select new
            {
                User = user,
                Profile = profile,
                Character = character,
            };

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var normalizedKeyword = keyword.Trim();
            query = query.Where(x =>
                x.User.Email.Contains(normalizedKeyword) ||
                (x.User.Phone != null && x.User.Phone.Contains(normalizedKeyword)) ||
                (x.Profile != null && x.Profile.FullName != null && x.Profile.FullName.Contains(normalizedKeyword)));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = status.Trim().ToLower();
            query = query.Where(x => x.User.Status.ToLower() == normalizedStatus);
        }

        if (!string.IsNullOrWhiteSpace(gender))
        {
            var normalizedGender = gender.Trim().ToLower();
            query = query.Where(x => x.Profile != null && x.Profile.Gender != null && x.Profile.Gender.ToLower() == normalizedGender);
        }

        if (profileCharacterId.HasValue)
        {
            query = query.Where(x => x.Profile != null && x.Profile.ProfileCharacterId == profileCharacterId.Value);
        }

        if (hasPhone.HasValue)
        {
            query = hasPhone.Value
                ? query.Where(x => x.User.Phone != null && x.User.Phone.Trim() != string.Empty)
                : query.Where(x => x.User.Phone == null || x.User.Phone.Trim() == string.Empty);
        }

        return query
            .OrderByDescending(x => x.User.CreatedAt)
            .ThenByDescending(x => x.User.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AdminUserListItemDto(
                x.User.Id,
                x.User.Email,
                x.User.Phone,
                x.User.Status,
                x.User.CreatedAt,
                x.User.UpdatedAt,
                x.Profile != null ? x.Profile.Id : null,
                x.Profile != null ? x.Profile.FullName : null,
                x.Profile != null ? x.Profile.AvatarUrl : null,
                x.Profile != null ? x.Profile.Gender : null,
                x.Profile != null ? x.Profile.Location : null,
                x.Profile != null ? x.Profile.ProfileCharacterId : null,
                x.Character != null ? x.Character.Name : null))
            .ToListAsync(cancellationToken);
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return _context.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }
}
