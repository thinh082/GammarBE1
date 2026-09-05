namespace GammarAPI.DTOs.Courses;

public sealed record CreateProductCategoryRequest(
    string Code,
    string Name,
    string? Description,
    int SortOrder);

public sealed record UpdateProductCategoryRequest(
    string Code,
    string Name,
    string? Description,
    int SortOrder,
    bool IsActive);
