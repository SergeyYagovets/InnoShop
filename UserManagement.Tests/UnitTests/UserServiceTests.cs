using Microsoft.AspNetCore.Http;
using Moq;
using UserManagement.Application.DTOs;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Services;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Exceptions;
using UserManagement.Infrastructure.Interfaces;

namespace UserManagement.Tests.UnitTests;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _mockRepository;
    private readonly Mock<IJwtTokenService> _mockJwtTokenService;
    private readonly Mock<HttpClient> _mockHttpClient;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _mockRepository = new Mock<IUserRepository>();
        _mockJwtTokenService = new Mock<IJwtTokenService>();
        _mockHttpClient = new Mock<HttpClient>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _userService = new UserService(
            _mockRepository.Object,
            _mockJwtTokenService.Object,
            _mockHttpClient.Object,
            _mockHttpContextAccessor.Object
        );
    }

    [Fact]
    public async Task GetById_UserExists_ReturnsUserDto()
    {
        // Arrange
        var userId = 1;
        var user = new User { Id = userId, Name = "John Doe", Email = "john@example.com" };
        _mockRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        var result = await _userService.GetById(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Name, result.Name);
        Assert.Equal(user.Email, result.Email);
    }

    [Fact]
    public async Task GetById_UserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var userId = 1;
        _mockRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync((User)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _userService.GetById(userId));
    }

    [Fact]
    public async Task GetAll_ReturnsListOfUserDtos()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Id = 1, Name = "John Doe", Email = "john@example.com" },
            new User { Id = 2, Name = "Jane Doe", Email = "jane@example.com" }
        };
        _mockRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(users);

        // Act
        var result = await _userService.GetAll();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task Create_UserWithEmailExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var userDto = new UserRegisterDto("John Doe","john@example.com","password");
        _mockRepository.Setup(repo => repo.GetByEmailAsync(userDto.Email)).ReturnsAsync(new User());

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.Create(userDto));
    }

    [Fact]
    public async Task Create_ValidUser_ReturnsJwtToken()
    {
        // Arrange
        var userDto = new UserRegisterDto("John Doe","john@example.com","password");
        _mockRepository.Setup(repo => repo.GetByEmailAsync(userDto.Email)).ReturnsAsync((User)null);
        _mockJwtTokenService.Setup(service => service.GenerateToken(It.IsAny<User>())).Returns("jwt_token");

        // Act
        var result = await _userService.Create(userDto);

        // Assert
        Assert.Equal("jwt_token", result);
    }

    [Fact]
    public async Task Update_ValidUser_ReturnsTrue()
    {
        // Arrange
        var userId = 1;
        var userDto = new UserUpdateDto("John Doe Updated");
        var user = new User { Id = userId, Name = "John Doe", Email = "john@example.com" };

        _mockHttpContextAccessor.Setup(accessor => accessor.HttpContext.Request.Headers["Authorization"])
            .Returns("Bearer valid_token");
        _mockJwtTokenService.Setup(service => service.GetUserIdFromToken("valid_token")).Returns(userId);
        _mockRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        var result = await _userService.Update(userDto);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task Update_InvalidToken_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var userDto = new UserUpdateDto("John Doe Updated");
        _mockHttpContextAccessor.Setup(accessor => accessor.HttpContext.Request.Headers["Authorization"])
            .Returns("");

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _userService.Update(userDto));
    }

    [Fact]
    public async Task DeleteById_UserExists_DeletesUser()
    {
        // Arrange
        var userId = 1;
        var user = new User { Id = userId, Name = "John Doe", Email = "john@example.com" };
        _mockRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        await _userService.DeleteById(userId);

        // Assert
        _mockRepository.Verify(repo => repo.DeleteAsync(userId), Times.Once);
    }

    [Fact]
    public async Task DeleteById_UserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var userId = 1;
        _mockRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync((User)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _userService.DeleteById(userId));
    }
}