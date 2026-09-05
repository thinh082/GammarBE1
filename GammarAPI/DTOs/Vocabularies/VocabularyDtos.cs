namespace GammarAPI.DTOs.Vocabularies;

public sealed record VocabularyDto(
    long Id,
    string Kanji,
    string? Kana,
    string MeaningVi,
    string? LevelCode,
    string? CategoryCode,
    string? ExampleText,
    string? ExampleMeaningVi,
    int SortOrder,
    bool IsActive,
    bool IsFavorite);

public sealed record UserFavoriteVocabularyDto(
    long Id,
    long UserId,
    long VocabularyId,
    DateTime CreatedAt,
    VocabularyDto Vocabulary);
