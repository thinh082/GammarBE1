namespace GammarDomain.Entities;

public class UserAssessmentResult
{
    public long Id { get; private set; }
    public long UserId { get; private set; }
    public string RecommendedLevel { get; private set; } = "N5";
    public int TotalScore { get; private set; }
    public int MaxScore { get; private set; }
    public DateTime TakenAt { get; private set; }

    public User? User { get; private set; }

    private UserAssessmentResult()
    {
    }

    public UserAssessmentResult(long userId, string recommendedLevel, int totalScore, int maxScore)
    {
        UserId = userId;
        RecommendedLevel = recommendedLevel.Trim();
        TotalScore = totalScore;
        MaxScore = maxScore;
        TakenAt = DateTime.UtcNow;
    }
}
