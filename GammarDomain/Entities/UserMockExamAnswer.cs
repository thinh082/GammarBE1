namespace GammarDomain.Entities;

public class UserMockExamAnswer
{
    public long Id { get; private set; }
    public long AttemptId { get; private set; }
    public long QuestionId { get; private set; }
    public long? SelectedOptionId { get; private set; }
    public bool IsCorrect { get; private set; }
    public int PointsAwarded { get; private set; }

    public UserMockExamAttempt? Attempt { get; private set; }
    public MockExamQuestion? Question { get; private set; }
    public MockExamOption? SelectedOption { get; private set; }

    private UserMockExamAnswer()
    {
    }

    public UserMockExamAnswer(long attemptId, long questionId, long? selectedOptionId, bool isCorrect, int pointsAwarded)
    {
        AttemptId = attemptId;
        QuestionId = questionId;
        SelectedOptionId = selectedOptionId;
        IsCorrect = isCorrect;
        PointsAwarded = pointsAwarded;
    }
}
