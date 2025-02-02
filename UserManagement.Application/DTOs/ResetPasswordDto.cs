namespace UserManagement.Application.DTOs;

public record UserResetPasswordDto(string Email, string NewPassword, string ResetToken);