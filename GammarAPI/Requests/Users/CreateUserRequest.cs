namespace GammarAPI.DTOs.Users;

public sealed record CreateUserDto(string Email, string Password, string? FullName);
