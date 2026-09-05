namespace GammarDomain.Entities;

public class MockExamOption
{
    public long Id { get; private set; }
    public long QuestionId { get; private set; }
    public string OptionText { get; private set; } = string.Empty;
    public bool IsCorrect { get; private set; }
    public int OrderIndex { get; private set; } = 1;

    public MockExamQuestion? Question { get; private set; }

    private MockExamOption()
    {
    }

    public MockExamOption(long questionId, string optionText, bool isCorrect = false, int orderIndex = 1)
    {
        QuestionId = questionId;
        OptionText = optionText.Trim();
        IsCorrect = isCorrect;
        OrderIndex = orderIndex;
    }
}
