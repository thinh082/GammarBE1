using GammarDomain.Entities;

namespace GammarApplication.Interfaces;

public interface IProfileRepository
{
    Task<Profile?> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default);
    Task<Profile?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task AddAsync(Profile profile, CancellationToken cancellationToken = default);
    void Update(Profile profile);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}