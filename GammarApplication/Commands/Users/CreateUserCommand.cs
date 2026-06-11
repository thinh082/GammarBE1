namespace GammarApplication.Commands.Users;

public sealed record CreateUserCommand(string Email, string PasswordHash, string? FullName);
