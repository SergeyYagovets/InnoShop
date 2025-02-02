using UserManagement.Application.DTOs;

namespace UserManagement.Application.Interfaces;

public interface IAuthService
{
    Task<string> Register(UserRegisterDto dto);
    Task<LoginResponseDto> Login(LoginDto dto);
    
    Task<string> ForgotPassword(string email);
    Task ResetPassword(ResetPasswordDto model);
}