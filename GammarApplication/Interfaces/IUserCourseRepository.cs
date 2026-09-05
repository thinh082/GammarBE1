using GammarDomain.Entities;

namespace GammarApplication.Interfaces;

public interface IUserCourseRepository
{
    Task<List<UserCourse>> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default);
    Task<UserCourse?> GetByUserAndCourseIdAsync(long userId, long courseId, CancellationToken cancellationToken = default);
    Task AddAsync(UserCourse userCourse, CancellationToken cancellationToken = default);
    void Update(UserCourse userCourse);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
