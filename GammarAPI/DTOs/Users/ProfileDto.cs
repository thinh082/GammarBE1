namespace GammarAPI.DTOs.Users;

public sealed record ProfileDto(
    long Id,
    long UserId,
    long ProfileCharacterId,
    string? FullName,
    string? AvatarUrl,
    string? Bio,
    DateOnly? Birthday,
    string? Gender,
    string? Location);
