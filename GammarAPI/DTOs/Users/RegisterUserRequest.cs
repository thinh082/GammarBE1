namespace GammarAPI.DTOs.Users;

public sealed record RegisterUserRequest(string Email, string Password, string? Phone, string? FullName);
