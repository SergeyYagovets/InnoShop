using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using UserManagement.Application.Services;
using UserManagement.Domain.Entities;

namespace UserManagement.Tests.UnitTests;

public class JwtTokenServiceTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly JwtTokenService _jwtTokenService;

    public JwtTokenServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockConfiguration.Setup(config => config["Jwt:Key"]).Returns("zjQ4ViEpJQsdfD2sXZBz3f5Rn7HqrXpJXk12o3YUVpM=");
        _mockConfiguration.Setup(config => config["Jwt:Issuer"]).Returns("InnoShop");
        _mockConfiguration.Setup(config => config["Jwt:Audience"]).Returns("InnoShopUsers");

        _jwtTokenService = new JwtTokenService(_mockConfiguration.Object);
    }

    [Fact]
    public void GenerateToken_ValidUser_ReturnsValidToken()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Name = "John Doe",
            Email = "john@example.com",
            Role = "Admin"
        };

        // Act
        var token = _jwtTokenService.GenerateToken(user);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        Assert.Equal(user.Id.ToString(), jwtToken.Subject);
        Assert.Equal(user.Name, jwtToken.Claims.First(c => c.Type == ClaimTypes.Name).Value);
        Assert.Equal(user.Email, jwtToken.Claims.First(c => c.Type == ClaimTypes.Email).Value);
        Assert.Equal(user.Role, jwtToken.Claims.First(c => c.Type == ClaimTypes.Role).Value);
    }

    [Fact]
    public void GenerateGuidToken_ReturnsValidGuid()
    {
        // Act
        var token = _jwtTokenService.GenerateGuidToken();

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        Assert.True(Guid.TryParse(token, out _));
    }

    [Fact]
    public void GetUserIdFromToken_ValidToken_ReturnsUserId()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Name = "John Doe",
            Email = "john@example.com",
            Role = "Admin"
        };

        var token = _jwtTokenService.GenerateToken(user);

        // Act
        var userId = _jwtTokenService.GetUserIdFromToken(token);

        // Assert
        Assert.NotNull(userId);
        Assert.Equal(user.Id, userId);
    }

    [Fact]
    public void GetUserIdFromToken_InvalidToken_ReturnsNull()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var userId = _jwtTokenService.GetUserIdFromToken(invalidToken);

        // Assert
        Assert.Null(userId);
    }

    [Fact]
    public void GetUserIdFromToken_ExpiredToken_ReturnsNull()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Name = "John Doe",
            Email = "john@example.com",
            Role = "Admin"
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_mockConfiguration.Object["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiredToken = new JwtSecurityToken(
            _mockConfiguration.Object["Jwt:Issuer"],
            _mockConfiguration.Object["Jwt:Audience"],
            new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            },
            expires: DateTime.Now.AddHours(-1),
            signingCredentials: creds);

        var expiredTokenString = new JwtSecurityTokenHandler().WriteToken(expiredToken);

        // Act
        var userId = _jwtTokenService.GetUserIdFromToken(expiredTokenString);

        // Assert
        Assert.Null(userId);
    }
}