using GammarApplication.Interfaces;
using GammarDomain.Entities;
using GammarInfrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GammarInfrastructure.Repositories;

public class LessonRepository : ILessonRepository
{
    private readonly AppDbContext _context;

    public LessonRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Lesson?> GetAggregateByIdAsync(long lessonId, CancellationToken cancellationToken = default)
    {
        return _context.Lessons
            .Include(x => x.Videos.Where(v => v.IsActive))
            .Include(x => x.Texts.Where(t => t.IsActive))
            .Include(x => x.Quiz!)
                .ThenInclude(q => q.Questions)
                    .ThenInclude(question => question.Options)
            .AsSplitQuery()
            .FirstOrDefaultAsync(
                x => x.Id == lessonId &&
                     x.IsActive &&
                     x.Course != null &&
                     x.Course.IsPublished,
                cancellationToken);
    }
}
