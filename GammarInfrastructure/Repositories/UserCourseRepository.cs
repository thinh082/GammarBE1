using GammarApplication.Interfaces;
using GammarDomain.Entities;
using GammarInfrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GammarInfrastructure.Repositories;

public class UserCourseRepository : IUserCourseRepository
{
    private readonly AppDbContext _context;
    private readonly DbSet<UserCourse> _dbSet;

    public UserCourseRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<UserCourse>();
    }

    public Task<List<UserCourse>> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default)
    {
        return _dbSet
            .Include(x => x.Course)
                .ThenInclude(x => x!.ProductCategory)
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<UserCourse?> GetByUserAndCourseIdAsync(long userId, long courseId, CancellationToken cancellationToken = default)
    {
        return _dbSet
            .Include(x => x.Course)
                .ThenInclude(x => x!.ProductCategory)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.CourseId == courseId, cancellationToken);
    }

    public Task AddAsync(UserCourse userCourse, CancellationToken cancellationToken = default)
    {
        return _dbSet.AddAsync(userCourse, cancellationToken).AsTask();
    }

    public void Update(UserCourse userCourse)
    {
        _dbSet.Update(userCourse);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
