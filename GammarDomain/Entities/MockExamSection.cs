namespace GammarDomain.Entities;

public class MockExamSection
{
    public long Id { get; private set; }
    public long MockExamId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public int OrderIndex { get; private set; } = 1;
    public int? TimeLimitMinutes { get; private set; }

    public MockExam? MockExam { get; private set; }
    public ICollection<MockExamQuestion> Questions { get; private set; } = [];

    private MockExamSection()
    {
    }

    public MockExamSection(long mockExamId, string title, int orderIndex = 1, int? timeLimitMinutes = null)
    {
        MockExamId = mockExamId;
        Title = title.Trim();
        OrderIndex = orderIndex;
        TimeLimitMinutes = timeLimitMinutes;
    }
}
