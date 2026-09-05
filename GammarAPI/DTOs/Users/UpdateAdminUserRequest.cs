namespace GammarAPI.DTOs.Users;

public sealed record UpdateAdminUserRequest(
    string? Phone,
    string Status,
    string? FullName,
    string? AvatarUrl,
    string? Bio,
    DateOnly? Birthday,
    string? Gender,
    string? Location,
    long ProfileCharacterId);
