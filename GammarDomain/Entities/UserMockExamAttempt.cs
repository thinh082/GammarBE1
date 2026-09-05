namespace GammarDomain.Entities;

public class UserMockExamAttempt
{
    public long Id { get; private set; }
    public long UserId { get; private set; }
    public long MockExamId { get; private set; }
    public int Score { get; private set; }
    public string Status { get; private set; } = "completed";
    public bool IsPassed { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? SubmittedAt { get; private set; }

    public User? User { get; private set; }
    public MockExam? MockExam { get; private set; }
    public ICollection<UserMockExamAnswer> Answers { get; private set; } = [];

    private UserMockExamAttempt()
    {
    }

    public UserMockExamAttempt(long userId, long mockExamId)
    {
        UserId = userId;
        MockExamId = mockExamId;
        Score = 0;
        Status = "in_progress";
        IsPassed = false;
        StartedAt = DateTime.UtcNow;
    }

    public void CompleteAttempt(int score, bool isPassed)
    {
        Score = score;
        Status = "completed";
        IsPassed = isPassed;
        SubmittedAt = DateTime.UtcNow;
    }
}
