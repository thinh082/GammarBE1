using GammarAPI.DTOs.Courses;
using GammarDomain.Entities;
using GammarInfrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GammarAPI.Controllers;

[ApiController]
[Route("api/users/{userId:long}")]
public sealed class UserLessonProgressController : ControllerBase
{
    private readonly AppDbContext _context;

    public UserLessonProgressController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("courses/{courseId:long}/lesson-progress")]
    public async Task<IActionResult> GetCourseLessonProgress(long userId, long courseId, CancellationToken cancellationToken)
    {
        var userExists = await _context.Users.AnyAsync(x => x.Id == userId, cancellationToken);
        if (!userExists)
        {
            return NotFound(new { message = "User not found" });
        }

        var courseExists = await _context.Courses.AnyAsync(x => x.Id == courseId, cancellationToken);
        if (!courseExists)
        {
            return NotFound(new { message = "Course not found" });
        }

        var summary = await BuildCourseLessonProgressSummaryAsync(userId, courseId, cancellationToken);
        return Ok(summary);
    }

    [HttpPatch("lessons/{lessonId:long}/progress")]
    public async Task<IActionResult> UpdateLessonProgress(
        long userId,
        long lessonId,
        [FromBody] UpdateUserLessonProgressRequest request,
        CancellationToken cancellationToken)
    {
        if (request.ProgressPercent is < 0 or > 100)
        {
            return BadRequest(new { message = "ProgressPercent must be between 0 and 100" });
        }

        if (request.LastPositionSeconds < 0)
        {
            return BadRequest(new { message = "LastPositionSeconds must be greater than or equal to 0" });
        }

        var userExists = await _context.Users.AnyAsync(x => x.Id == userId, cancellationToken);
        if (!userExists)
        {
            return NotFound(new { message = "User not found" });
        }

        var lesson = await _context.Lessons
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == lessonId, cancellationToken);
        if (lesson is null)
        {
            return NotFound(new { message = "Lesson not found" });
        }

        if (request.LastVideoId.HasValue)
        {
            var videoExists = await _context.LessionVideos
                .AsNoTracking()
                .AnyAsync(x => x.Id == request.LastVideoId.Value && x.LessonId == lessonId, cancellationToken);
            if (!videoExists)
            {
                return BadRequest(new { message = "LastVideoId does not belong to this lesson" });
            }
        }

        var lessonProgress = await _context.UserLessonProgresses
            .FirstOrDefaultAsync(x => x.UserId == userId && x.LessonId == lessonId, cancellationToken);

        if (lessonProgress is null)
        {
            lessonProgress = new UserLessonProgress(userId, lessonId);
            _context.UserLessonProgresses.Add(lessonProgress);
        }

        lessonProgress.Sync(
            request.ProgressPercent,
            request.Status,
            request.LastVideoId,
            request.LastPositionSeconds,
            request.MarkCompleted);

        await _context.SaveChangesAsync(cancellationToken);
        var courseProgress = await RecalculateAndPersistCourseProgressAsync(userId, lesson.CourseId, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new UpdateUserLessonProgressResultDto(
            courseProgress.CourseProgressPercent,
            courseProgress.CompletedLessons,
            courseProgress.TotalLessons,
            courseProgress.CurrentLessonId,
            MapLessonProgress(lessonProgress)));
    }

    private async Task<UserCourseLessonProgressSummaryDto> BuildCourseLessonProgressSummaryAsync(long userId, long courseId, CancellationToken cancellationToken)
    {
        var courseLessons = await _context.Lessons
            .AsNoTracking()
            .Where(x => x.CourseId == courseId && x.IsActive)
            .Select(x => new { x.Id })
            .ToListAsync(cancellationToken);

        var lessonIds = courseLessons.Select(x => x.Id).ToHashSet();
        var progressItems = await _context.UserLessonProgresses
            .AsNoTracking()
            .Where(x => x.UserId == userId && lessonIds.Contains(x.LessonId))
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(cancellationToken);

        var completedLessons = progressItems.Count(x => string.Equals(x.Status, "completed", StringComparison.OrdinalIgnoreCase));
        var totalLessons = courseLessons.Count;
        var courseProgressPercent = totalLessons == 0
            ? 0m
            : decimal.Round(completedLessons * 100m / totalLessons, 2, MidpointRounding.AwayFromZero);

        return new UserCourseLessonProgressSummaryDto(
            courseId,
            courseProgressPercent,
            completedLessons,
            totalLessons,
            progressItems.FirstOrDefault()?.LessonId,
            progressItems.Select(MapLessonProgress).ToList());
    }

    private async Task<(decimal CourseProgressPercent, int CompletedLessons, int TotalLessons, long? CurrentLessonId)> RecalculateAndPersistCourseProgressAsync(
        long userId,
        long courseId,
        CancellationToken cancellationToken)
    {
        var courseLessons = await _context.Lessons
            .AsNoTracking()
            .Where(x => x.CourseId == courseId && x.IsActive)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var lessonIds = courseLessons.ToHashSet();
        var progressItems = await _context.UserLessonProgresses
            .Where(x => x.UserId == userId && lessonIds.Contains(x.LessonId))
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(cancellationToken);

        var completedLessons = progressItems.Count(x => string.Equals(x.Status, "completed", StringComparison.OrdinalIgnoreCase));
        var totalLessons = courseLessons.Count;
        var courseProgressPercent = totalLessons == 0
            ? 0m
            : decimal.Round(completedLessons * 100m / totalLessons, 2, MidpointRounding.AwayFromZero);

        var userCourse = await _context.UserCourses
            .FirstOrDefaultAsync(x => x.UserId == userId && x.CourseId == courseId, cancellationToken);
        if (userCourse is not null)
        {
            if (progressItems.Count > 0)
            {
                userCourse.MarkStarted();
            }

            userCourse.UpdateProgress(courseProgressPercent);
        }

        return (courseProgressPercent, completedLessons, totalLessons, progressItems.FirstOrDefault()?.LessonId);
    }

    private static UserLessonProgressDto MapLessonProgress(UserLessonProgress progress)
    {
        return new UserLessonProgressDto(
            progress.Id,
            progress.UserId,
            progress.LessonId,
            progress.Status,
            progress.ProgressPercent,
            progress.LastVideoId,
            progress.LastPositionSeconds,
            progress.CompletedAt,
            progress.UpdatedAt);
    }
}
