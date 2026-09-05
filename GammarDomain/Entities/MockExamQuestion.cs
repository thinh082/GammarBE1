namespace GammarDomain.Entities;

public class MockExamQuestion
{
    public long Id { get; private set; }
    public long SectionId { get; private set; }
    public string QuestionText { get; private set; } = string.Empty;
    public string? AudioUrl { get; private set; }
    public string? ImageUrl { get; private set; }
    public string? Explanation { get; private set; }
    public int Points { get; private set; } = 1;
    public int OrderIndex { get; private set; } = 1;

    public MockExamSection? Section { get; private set; }
    public ICollection<MockExamOption> Options { get; private set; } = [];

    private MockExamQuestion()
    {
    }

    public MockExamQuestion(long sectionId, string questionText, int points = 1, int orderIndex = 1, string? audioUrl = null, string? imageUrl = null, string? explanation = null)
    {
        SectionId = sectionId;
        QuestionText = questionText.Trim();
        Points = points;
        OrderIndex = orderIndex;
        AudioUrl = audioUrl?.Trim();
        ImageUrl = imageUrl?.Trim();
        Explanation = explanation?.Trim();
    }
}
