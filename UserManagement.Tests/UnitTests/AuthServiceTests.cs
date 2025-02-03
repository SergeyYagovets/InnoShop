using Moq;
using UserManagement.Application.DTOs;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Services;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Exceptions;
using UserManagement.Infrastructure.Interfaces;

namespace UserManagement.Tests.UnitTests;

public class AuthServiceTests
{
    private readonly Mock<IJwtTokenService> _mockJwtTokenService;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockJwtTokenService = new Mock<IJwtTokenService>();
        _mockUserRepository = new Mock<IUserRepository>();
        _authService = new AuthService(_mockJwtTokenService.Object, _mockUserRepository.Object);
    }

    [Fact]
    public async Task Register_NewUser_ReturnsToken()
    {
        // Arrange
        var userDto = new UserRegisterDto("John Doe","john@example.com","password");

        _mockUserRepository.Setup(repo => repo.GetByEmailAsync(userDto.Email)).ReturnsAsync((User)null);
        _mockUserRepository.Setup(repo => repo.AnyUsersAsync()).ReturnsAsync(false);
        _mockJwtTokenService.Setup(service => service.GenerateGuidToken()).Returns("guid_token");
        _mockJwtTokenService.Setup(service => service.GenerateToken(It.IsAny<User>())).Returns("jwt_token");

        // Act
        var result = await _authService.Register(userDto);

        // Assert
        Assert.Equal("jwt_token", result);
        _mockUserRepository.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task Register_ExistingUser_ThrowsInvalidOperationException()
    {
        // Arrange
        var userDto = new UserRegisterDto("John Doe","john@example.com","password");

        _mockUserRepository.Setup(repo => repo.GetByEmailAsync(userDto.Email)).ReturnsAsync(new User());

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _authService.Register(userDto));
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        // Arrange
        var loginDto = new LoginDto("john@example.com", "password123");

        var user = new User
        {
            Email = loginDto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(loginDto.Password),
            IsEmailConfirmed = true
        };

        _mockUserRepository.Setup(repo => repo.GetByEmailAsync(loginDto.Email)).ReturnsAsync(user);
        _mockJwtTokenService.Setup(service => service.GenerateToken(user)).Returns("jwt_token");

        // Act
        var result = await _authService.Login(loginDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("jwt_token", result.Token);
    }

    [Fact]
       public async Task Login_InvalidCredentials_ThrowsUnauthorizedAccessException()
       {
           // Arrange
           var loginDto = new LoginDto("john@example.com", "wrong_password");
           
           var user = new User
           {
               Email = loginDto.Email,
               PasswordHash = BCrypt.Net.BCrypt.HashPassword("correct_password"), // Хэш правильного пароля
               IsEmailConfirmed = true
           };
       
           _mockUserRepository.Setup(repo => repo.GetByEmailAsync(loginDto.Email)).ReturnsAsync(user);
       
           // Act & Assert
           await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.Login(loginDto));
       }

    [Fact]
    public async Task Login_EmailNotConfirmed_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var loginDto = new LoginDto("john@example.com", "password123");

        var user = new User
        {
            Email = loginDto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(loginDto.Password),
            IsEmailConfirmed = false
        };

        _mockUserRepository.Setup(repo => repo.GetByEmailAsync(loginDto.Email)).ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.Login(loginDto));
    }

    [Fact]
    public async Task ForgotPassword_ValidEmail_ReturnsToken()
    {
        // Arrange
        var email = "john@example.com";
        var user = new User
        {
            Email = email,
            IsEmailConfirmed = true
        };

        _mockUserRepository.Setup(repo => repo.GetByEmailAsync(email)).ReturnsAsync(user);
        _mockJwtTokenService.Setup(service => service.GenerateToken(user)).Returns("reset_token");

        // Act
        var result = await _authService.ForgotPassword(email);

        // Assert
        Assert.Equal("reset_token", result);
        _mockUserRepository.Verify(repo => repo.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task ForgotPassword_UserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var email = "john@example.com";
        _mockUserRepository.Setup(repo => repo.GetByEmailAsync(email)).ReturnsAsync((User)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _authService.ForgotPassword(email));
    }

    [Fact]
    public async Task ForgotPassword_EmailNotConfirmed_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var email = "john@example.com";
        var user = new User
        {
            Email = email,
            IsEmailConfirmed = false
        };

        _mockUserRepository.Setup(repo => repo.GetByEmailAsync(email)).ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.ForgotPassword(email));
    }

    [Fact]
    public async Task ResetPassword_ValidToken_UpdatesPassword()
    {
        // Arrange
        var resetPasswordDto = new ResetPasswordDto("john@example.com", "new_password123", "valid_token");

        var user = new User
        {
            Email = resetPasswordDto.Email,
            ResetPasswordToken = "valid_token",
            ResetPasswordTokenExpiry = DateTime.UtcNow.AddHours(1)
        };

        _mockUserRepository.Setup(repo => repo.GetByEmailAsync(resetPasswordDto.Email)).ReturnsAsync(user);

        // Act
        await _authService.ResetPassword(resetPasswordDto);

        // Assert
        _mockUserRepository.Verify(repo => repo.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task ResetPassword_InvalidToken_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var resetPasswordDto = new ResetPasswordDto("john@example.com", "new_password123", "invalid_token");

        var user = new User
        {
            Email = resetPasswordDto.Email,
            ResetPasswordToken = "valid_token",
            ResetPasswordTokenExpiry = DateTime.UtcNow.AddHours(1)
        };

        _mockUserRepository.Setup(repo => repo.GetByEmailAsync(resetPasswordDto.Email)).ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.ResetPassword(resetPasswordDto));
    }

    [Fact]
    public async Task ResetPassword_ExpiredToken_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var resetPasswordDto = new ResetPasswordDto("john@example.com", "new_password123", "wrong_token");

        var user = new User
        {
            Email = resetPasswordDto.Email,
            ResetPasswordToken = "expired_token",
            ResetPasswordTokenExpiry = DateTime.UtcNow.AddHours(-1)
        };

        _mockUserRepository.Setup(repo => repo.GetByEmailAsync(resetPasswordDto.Email)).ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.ResetPassword(resetPasswordDto));
    }
}