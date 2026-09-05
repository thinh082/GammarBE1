namespace GammarAPI.DTOs.Courses;

public sealed record ProductCategoryDto(
    long Id,
    string Code,
    string Name,
    string? Description,
    int SortOrder,
    bool IsActive);
