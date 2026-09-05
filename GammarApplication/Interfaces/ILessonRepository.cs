using GammarDomain.Entities;

namespace GammarApplication.Interfaces;

public interface ILessonRepository
{
    Task<Lesson?> GetAggregateByIdAsync(long lessonId, CancellationToken cancellationToken = default);
}
