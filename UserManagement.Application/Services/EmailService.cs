using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using UserManagement.Application.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Infrastructure.Interfaces;

namespace UserManagement.Application.Services;

public class EmailService : IEmailService
{
    private readonly IUserRepository _repository;
    private readonly MailSettings _mailSettings;

    public EmailService(IUserRepository repository, IOptions<MailSettings> mailSettings)
    {
        _repository = repository;
        _mailSettings = mailSettings.Value;
    }

    public async Task SendConfirmEmail(string email, string emailBodyUrl)
    {
        var subject = "Email confirmation";
        var emailBody = $"To confirm your email <a href=\"{emailBodyUrl}\">click here </a> ";
        await SendEmail(email, subject, emailBody);
    }

    public async Task ConfirmEmailByToken(string email, string token)
    {
        var user = await _repository.GetByEmailAsync(email);
        
        user.IsEmailConfirmed = true;
        
        await _repository.UpdateAsync(user);
    }
    
    public async Task SendResetPasswordEmail(string email, string emailBodyUrl)
    {
        var subject = "Password reset";
        var emailBody = $"To reset your password <a href=\"{emailBodyUrl}\">click here </a> ";
        await SendEmail(email, subject, emailBody);
    }
    
    private async Task SendEmail(string email, string subject, string message)
    {
        if (string.IsNullOrEmpty(email))
        {
            throw new ArgumentException("Email address cannot be null or empty", nameof(email));
        }
        
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress(_mailSettings.SenderName, _mailSettings.SenderEmail));
        emailMessage.To.Add(new MailboxAddress("",email));
        emailMessage.Subject = subject;
        emailMessage.Body = new TextPart("plain")
        {
            Text = message
        };

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(_mailSettings.SmtpServer, _mailSettings.SmtpPort, MailKit.Security.SecureSocketOptions.None);
            await client.SendAsync(emailMessage);
        }
        catch
        {
            throw new Exception("Something went wrong");
        }
        finally
        {
            await client.DisconnectAsync(true);
        }
    }
}