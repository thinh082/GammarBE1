namespace GammarAPI.DTOs.Users;

public sealed record ProfileDetailDto(
    long Id,
    long UserId,
    long ProfileCharacterId,
    string? FullName,
    string? Phone,
    string? AvatarUrl,
    string? Bio,
    DateOnly? Birthday,
    string? Gender,
    string? Location,
    ProfileCharacterDto Character);

public sealed record ProfileCharacterDto(
    long Id,
    string Name,
    string Prompt,
    string? Description);
