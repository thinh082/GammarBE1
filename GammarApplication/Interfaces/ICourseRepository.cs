using GammarDomain.Entities;

namespace GammarApplication.Interfaces;

public interface ICourseRepository
{
    Task<List<Course>> GetFilteredAsync(
        string? keyword,
        string? categoryCode,
        string? levelCode,
        bool? isFree,
        bool? isHot,
        bool? isPublished,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<Course?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Course?> GetPublishedByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Course?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<Course?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<List<Lesson>> GetPublishedLessonsAsync(long courseId, CancellationToken cancellationToken = default);
    Task AddAsync(Course course, CancellationToken cancellationToken = default);
    void Update(Course course);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
