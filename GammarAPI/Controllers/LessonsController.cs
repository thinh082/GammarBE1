using GammarAPI.DTOs.Courses;
using GammarApplication.Interfaces;
using GammarDomain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace GammarAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LessonsController : ControllerBase
{
    private readonly ILessonRepository _lessonRepository;

    public LessonsController(ILessonRepository lessonRepository)
    {
        _lessonRepository = lessonRepository;
    }

    [HttpGet("{lessonId:long}")]
    public async Task<IActionResult> GetById(long lessonId, CancellationToken cancellationToken)
    {
        var lesson = await _lessonRepository.GetAggregateByIdAsync(lessonId, cancellationToken);
        if (lesson is null)
        {
            return NotFound(new { message = "Lesson not found" });
        }

        return Ok(MapLessonDetail(lesson));
    }

    internal static LessonDetailDto MapLessonDetail(Lesson lesson)
    {
        var orderedVideos = lesson.Videos
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .Select(x => new LessonVideoDto(
                x.Id,
                x.Title,
                x.VideoUrl,
                x.VideoProvider,
                x.DurationSeconds,
                x.TranscriptText,
                x.SubtitleUrl,
                x.SortOrder))
            .ToList();

        var orderedTexts = lesson.Texts
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .Select(x => new LessonTextDto(
                x.Id,
                x.Title,
                x.ContentText,
                x.ContentHtml,
                x.AttachmentUrl,
                x.SortOrder))
            .ToList();

        LessonQuizDto? quiz = null;
        if (lesson.Quiz is not null && lesson.Quiz.IsActive)
        {
            quiz = new LessonQuizDto(
                lesson.Quiz.Id,
                lesson.Quiz.Title,
                lesson.Quiz.Description,
                lesson.Quiz.PassingScore ?? 70m,
                lesson.Quiz.TimeLimitMinutes,
                lesson.Quiz.MaxAttempts,
                lesson.Quiz.Questions
                    .OrderBy(x => x.SortOrder)
                    .ThenBy(x => x.Id)
                    .Select(x => new LessonQuizQuestionDto(
                        x.Id,
                        x.QuestionText,
                        x.ExplanationText,
                        x.SortOrder,
                        x.Options
                            .OrderBy(option => option.SortOrder)
                            .ThenBy(option => option.Id)
                            .Select(option => new LessonQuizOptionDto(
                                option.Id,
                                option.OptionLabel,
                                option.OptionText,
                                option.SortOrder))
                            .ToList()))
                    .ToList());
        }

        return new LessonDetailDto(
            lesson.Id,
            lesson.CourseId,
            lesson.Code,
            lesson.Title,
            lesson.LessonKind,
            lesson.ShortDescription,
            lesson.SortOrder,
            lesson.IsPreview,
            orderedVideos,
            orderedTexts,
            quiz);
    }
}
