namespace GammarApplication.DTOs.Users;

public sealed record AdminUserListItemDto(
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
