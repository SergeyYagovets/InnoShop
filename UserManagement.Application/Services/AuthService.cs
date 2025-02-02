using UserManagement.Application.DTOs;
using UserManagement.Application.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Exceptions;
using UserManagement.Infrastructure.Interfaces;

namespace UserManagement.Application.Services;

public class AuthService : IAuthService
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUserRepository _repository;

    public AuthService(IJwtTokenService jwtTokenService, IUserRepository repository)
    {
        _jwtTokenService = jwtTokenService;
        _repository = repository;
    }
    
    public async Task<string> Register(UserRegisterDto dto)
    {
        if (await _repository.GetByEmailAsync(dto.Email) != null)
            throw new InvalidOperationException("User with this email already exists");
        
        var isFirstUser = !(await _repository.AnyUsersAsync());
        var token = _jwtTokenService.GenerateGuidToken();
        
        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            Role = isFirstUser ? "Admin" : "User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            EmailConfirmationToken = token
        };

        await _repository.AddAsync(user);
        return _jwtTokenService.GenerateToken(user);
    }

    public async Task<LoginResponseDto> Login(LoginDto dto)
    {
        var user = await _repository.GetByEmailAsync(dto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }
        if (!user.IsEmailConfirmed)
        {
            throw new UnauthorizedAccessException("Email is not confirmed");
        }

        var token = _jwtTokenService.GenerateToken(user);
        return new LoginResponseDto(token);
    }

    public async Task<string> ForgotPassword(string email)
    {
        var user = await _repository.GetByEmailAsync(email);
        
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }
        
        if (!user.IsEmailConfirmed)
        {
            throw new UnauthorizedAccessException("Email is not confirmed");
        }

        var token = _jwtTokenService.GenerateToken(user);

        user.ResetPasswordToken = token;
        user.ResetPasswordTokenExpiry = DateTime.UtcNow.AddHours(1);
        await _repository.UpdateAsync(user);

        return token;
    }

    public async Task ResetPassword(ResetPasswordDto model)
    {
        var user = await _repository.GetByEmailAsync(model.Email);
        
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        if (user.ResetPasswordToken != model.ResetToken)
        {
            throw new UnauthorizedAccessException("Invalid token");
        }

        if (user.ResetPasswordTokenExpiry < DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Expired token");
        }
        
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
        user.ResetPasswordToken = null;
        user.ResetPasswordTokenExpiry = null;
        
        await _repository.UpdateAsync(user);
    }
}