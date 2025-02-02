namespace UserManagement.Application.DTOs;

public record ResetPasswordDto(string Email, string NewPassword, string ResetToken);