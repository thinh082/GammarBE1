namespace GammarDomain.Entities;

public class MockExam
{
    public long Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Level { get; private set; } = "N5";
    public int DurationMinutes { get; private set; } = 105;
    public int PassingScore { get; private set; } = 90;
    public int TotalScore { get; private set; } = 180;
    public string? Description { get; private set; }
    public bool IsPublished { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public ICollection<MockExamSection> Sections { get; private set; } = [];
    public ICollection<UserMockExamAttempt> Attempts { get; private set; } = [];

    private MockExam()
    {
    }

    public MockExam(string title, string level, int durationMinutes, int passingScore, int totalScore, string? description = null)
    {
        Title = title.Trim();
        Level = level.Trim();
        DurationMinutes = durationMinutes;
        PassingScore = passingScore;
        TotalScore = totalScore;
        Description = description?.Trim();
        IsPublished = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
