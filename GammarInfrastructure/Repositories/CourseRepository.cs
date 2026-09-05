using GammarApplication.Interfaces;
using GammarDomain.Entities;
using GammarInfrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GammarInfrastructure.Repositories;

public class CourseRepository : ICourseRepository
{
    private readonly AppDbContext _context;
    private readonly DbSet<Course> _dbSet;

    public CourseRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<Course>();
    }

    public Task<List<Course>> GetFilteredAsync(
        string? keyword,
        string? categoryCode,
        string? levelCode,
        bool? isFree,
        bool? isHot,
        bool? isPublished,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(x => x.ProductCategory)
            .Where(x => x.ProductCategory != null && x.ProductCategory.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var normalizedKeyword = keyword.Trim();
            query = query.Where(x =>
                x.Code.Contains(normalizedKeyword) ||
                x.Slug.Contains(normalizedKeyword) ||
                x.Title.Contains(normalizedKeyword));
        }

        if (!string.IsNullOrWhiteSpace(categoryCode))
        {
            query = query.Where(x => x.ProductCategory!.Code == categoryCode);
        }

        if (!string.IsNullOrWhiteSpace(levelCode))
        {
            query = query.Where(x => x.LevelCode == levelCode);
        }

        if (isFree.HasValue)
        {
            query = query.Where(x => x.IsFree == isFree.Value);
        }

        if (isHot.HasValue)
        {
            query = query.Where(x => x.IsHot == isHot.Value);
        }

        if (isPublished.HasValue)
        {
            query = query.Where(x => x.IsPublished == isPublished.Value);
        }

        return query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<Course?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return _dbSet
            .Include(x => x.ProductCategory)
            .Include(x => x.Lessons)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<Course?> GetPublishedByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return _dbSet
            .Include(x => x.ProductCategory)
            .Include(x => x.Lessons)
            .FirstOrDefaultAsync(
                x => x.Id == id &&
                     x.IsPublished &&
                     x.ProductCategory != null &&
                     x.ProductCategory.IsActive,
                cancellationToken);
    }

    public Task<Course?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return _dbSet.FirstOrDefaultAsync(x => x.Code == code, cancellationToken);
    }

    public Task<Course?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return _dbSet.FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);
    }

    public Task<List<Lesson>> GetPublishedLessonsAsync(long courseId, CancellationToken cancellationToken = default)
    {
        return _context.Lessons
            .Where(x => x.CourseId == courseId && x.IsActive && x.Course != null && x.Course.IsPublished)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(Course course, CancellationToken cancellationToken = default)
    {
        return _dbSet.AddAsync(course, cancellationToken).AsTask();
    }

    public void Update(Course course)
    {
        _dbSet.Update(course);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
