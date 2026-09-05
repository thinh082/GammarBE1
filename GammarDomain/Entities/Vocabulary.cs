namespace GammarDomain.Entities;

public class Vocabulary
{
    public long Id { get; private set; }
    public string Kanji { get; private set; } = string.Empty;
    public string? Kana { get; private set; }
    public string MeaningVi { get; private set; } = string.Empty;
    public string? LevelCode { get; private set; }
    public string? CategoryCode { get; private set; }
    public string? ExampleText { get; private set; }
    public string? ExampleMeaningVi { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public ICollection<UserFavoriteVocabulary> UserFavoriteVocabularies { get; private set; } = [];

    private Vocabulary()
    {
    }

    public Vocabulary(
        string kanji,
        string? kana,
        string meaningVi,
        string? levelCode,
        string? categoryCode,
        string? exampleText,
        string? exampleMeaningVi,
        int sortOrder,
        bool isActive = true)
    {
        Kanji = kanji;
        Kana = kana;
        MeaningVi = meaningVi;
        LevelCode = levelCode;
        CategoryCode = categoryCode;
        ExampleText = exampleText;
        ExampleMeaningVi = exampleMeaningVi;
        SortOrder = sortOrder;
        IsActive = isActive;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(
        string kanji,
        string? kana,
        string meaningVi,
        string? levelCode,
        string? categoryCode,
        string? exampleText,
        string? exampleMeaningVi,
        int sortOrder,
        bool isActive)
    {
        Kanji = kanji;
        Kana = kana;
        MeaningVi = meaningVi;
        LevelCode = levelCode;
        CategoryCode = categoryCode;
        ExampleText = exampleText;
        ExampleMeaningVi = exampleMeaningVi;
        SortOrder = sortOrder;
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
