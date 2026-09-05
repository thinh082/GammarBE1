namespace GammarAPI.DTOs.Users;

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
