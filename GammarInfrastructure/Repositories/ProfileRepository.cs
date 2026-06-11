using GammarApplication.Interfaces;
using GammarDomain.Entities;
using GammarInfrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GammarInfrastructure.Repositories;

public class ProfileRepository : IProfileRepository
{
    private readonly AppDbContext _context;
    private readonly DbSet<Profile> _dbSet;

    public ProfileRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<Profile>();
    }

    public Task<Profile?> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default)
    {
        return _dbSet.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    public Task<Profile?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return _dbSet.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(Profile profile, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(profile, cancellationToken);
    }

    public void Update(Profile profile)
    {
        _dbSet.Update(profile);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}