using GammarAPI.DTOs.Courses;
using GammarApplication.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GammarAPI.Controllers;

[ApiController]
[Route("api/Lessons/{lessonId:long}/quiz")]
public class LessonQuizAttemptsController : ControllerBase
{
    private readonly ILessonRepository _lessonRepository;

    public LessonQuizAttemptsController(ILessonRepository lessonRepository)
    {
        _lessonRepository = lessonRepository;
    }

    [HttpPost("submit")]
    public async Task<IActionResult> Submit(long lessonId, [FromBody] SubmitLessonQuizRequest request, CancellationToken cancellationToken)
    {
        var lesson = await _lessonRepository.GetAggregateByIdAsync(lessonId, cancellationToken);
        if (lesson is null)
        {
            return NotFound(new { message = "Lesson not found" });
        }

        var quiz = lesson.Quiz;
        if (quiz is null || !quiz.IsActive)
        {
            return NotFound(new { message = "Quiz not found" });
        }

        var answers = request.Answers ?? [];
        var answerMap = answers
            .GroupBy(x => x.QuestionId)
            .ToDictionary(x => x.Key, x => x.Last().SelectedOptionId);

        var questionResults = new List<QuizSubmissionQuestionResultDto>();
        var correctCount = 0;

        foreach (var question in quiz.Questions.OrderBy(x => x.SortOrder).ThenBy(x => x.Id))
        {
            var correctOption = question.Options.FirstOrDefault(x => x.IsCorrect);
            if (correctOption is null)
            {
                return BadRequest(new { message = $"Question {question.Id} does not have a correct option configured" });
            }

            answerMap.TryGetValue(question.Id, out var selectedOptionId);
            if (selectedOptionId != 0 && question.Options.All(x => x.Id != selectedOptionId))
            {
                return BadRequest(new { message = $"Selected option {selectedOptionId} does not belong to question {question.Id}" });
            }

            var isCorrect = selectedOptionId == correctOption.Id;
            if (isCorrect)
            {
                correctCount++;
            }

            questionResults.Add(new QuizSubmissionQuestionResultDto(
                question.Id,
                selectedOptionId == 0 ? null : selectedOptionId,
                correctOption.Id,
                isCorrect,
                question.ExplanationText));
        }

        var totalQuestions = quiz.Questions.Count;
        var score = totalQuestions == 0 ? 0 : Math.Round((decimal)correctCount * 100 / totalQuestions, 2);
        var passingScore = quiz.PassingScore ?? 70m;
        var passed = score >= passingScore;

        return Ok(new QuizSubmissionResultDto(
            score,
            correctCount,
            totalQuestions,
            passed,
            questionResults));
    }
}
