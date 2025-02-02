namespace UserManagement.Application.Interfaces;

public interface IEmailService
{
    Task SendConfirmEmail(string email, string emailBodyUrl);
    Task ConfirmEmailByToken(string email, string token);
    Task SendResetPasswordEmail(string email, string emailBodyUrl);
}