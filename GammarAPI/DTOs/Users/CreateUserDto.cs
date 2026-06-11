namespace GammarAPI.DTOs.Users;

public sealed record CreateUserInputDto(string Email, string Password, string? FullName);
