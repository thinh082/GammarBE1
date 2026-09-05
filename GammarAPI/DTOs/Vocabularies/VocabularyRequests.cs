namespace GammarAPI.DTOs.Vocabularies;

public sealed record CreateVocabularyRequest(
    string Kanji,
    string? Kana,
    string MeaningVi,
    string? LevelCode,
    string? CategoryCode,
    string? ExampleText,
    string? ExampleMeaningVi,
    int SortOrder);

public sealed record UpdateVocabularyRequest(
    string Kanji,
    string? Kana,
    string MeaningVi,
    string? LevelCode,
    string? CategoryCode,
    string? ExampleText,
    string? ExampleMeaningVi,
    int SortOrder,
    bool IsActive);

public sealed record AssignUserFavoriteVocabularyRequest(long VocabularyId);
