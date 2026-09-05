namespace GammarDomain.Entities;

public class AssessmentOption
{
    public long Id { get; private set; }
    public long QuestionId { get; private set; }
    public string OptionText { get; private set; } = string.Empty;
    public bool IsCorrect { get; private set; }

    public AssessmentQuestion? Question { get; private set; }

    private AssessmentOption()
    {
    }

    public AssessmentOption(long questionId, string optionText, bool isCorrect = false)
    {
        QuestionId = questionId;
        OptionText = optionText.Trim();
        IsCorrect = isCorrect;
    }
}
