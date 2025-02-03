using Microsoft.Extensions.Options;
using Moq;
using UserManagement.Application.Services;
using UserManagement.Domain.Entities;
using UserManagement.Infrastructure.Interfaces;

namespace UserManagement.Tests.UnitTests;

public class EmailServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IOptions<MailSettings>> _mockMailSettings;
    private readonly EmailService _emailService;

    public EmailServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockMailSettings = new Mock<IOptions<MailSettings>>();

        var mailSettings = new MailSettings
        {
            SmtpServer = "smtp4dev",
            SmtpPort = 25,
            Username = "",
            Password = "",
            SenderEmail = "no-reply@inno-shop.com",
            SenderName = "Inno_Shop"
        };

        _mockMailSettings.Setup(ms => ms.Value).Returns(mailSettings);

        _emailService = new EmailService(_mockUserRepository.Object, _mockMailSettings.Object);
    }

    [Fact]
    public async Task SendConfirmEmail_EmptyEmail_ThrowsArgumentException()
    {
        // Arrange
        var email = "";
        var emailBodyUrl = "https://example.com/confirm-email";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _emailService.SendConfirmEmail(email, emailBodyUrl));
    }

    [Fact]
    public async Task ConfirmEmailByToken_ValidEmail_ConfirmsEmail()
    {
        // Arrange
        var email = "test@example.com";
        var token = "valid_token";
        var user = new User { Email = email, IsEmailConfirmed = false };

        _mockUserRepository.Setup(repo => repo.GetByEmailAsync(email)).ReturnsAsync(user);

        // Act
        await _emailService.ConfirmEmailByToken(email, token);

        // Assert
        Assert.True(user.IsEmailConfirmed);
        _mockUserRepository.Verify(repo => repo.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task ConfirmEmailByToken_InvalidEmail_ThrowsException()
    {
        // Arrange
        var email = "nonexistent@example.com";
        var token = "valid_token";

        _mockUserRepository.Setup(repo => repo.GetByEmailAsync(email)).ReturnsAsync((User)null);

        // Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(() => _emailService.ConfirmEmailByToken(email, token));
    }
}