namespace GammarDomain.Entities;

public class AssessmentQuestion
{
    public long Id { get; private set; }
    public string QuestionText { get; private set; } = string.Empty;
    public string Level { get; private set; } = "N5";
    public string? Explanation { get; private set; }
    public int OrderIndex { get; private set; } = 1;

    public ICollection<AssessmentOption> Options { get; private set; } = [];

    private AssessmentQuestion()
    {
    }

    public AssessmentQuestion(string questionText, string level = "N5", int orderIndex = 1, string? explanation = null)
    {
        QuestionText = questionText.Trim();
        Level = level.Trim();
        OrderIndex = orderIndex;
        Explanation = explanation?.Trim();
    }
}
