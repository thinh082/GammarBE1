namespace GammarAPI.DTOs.Users;

public sealed record AdminUserListItemResponse(
    long Id,
    string Email,
    string? Phone,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    long? ProfileId,
    string? FullName,
    string? AvatarUrl,
    string? Gender,
    string? Location,
    long? ProfileCharacterId,
    string? ProfileCharacterName);
