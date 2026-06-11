namespace GammarAPI.DTOs.Users;

public sealed record UpdateProfileRequest(
    string? FullName,
    string? AvatarUrl,
    string? Bio,
    DateOnly? Birthday,
    string? Gender,
    string? Location,
    long ProfileCharacterId);
