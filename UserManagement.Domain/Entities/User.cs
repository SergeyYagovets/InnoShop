using System.ComponentModel.DataAnnotations;

namespace UserManagement.Domain.Entities;

public class User
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public string Email { get; set; }
    
    public bool IsEmailConfirmed { get; set; }
    
    public string? EmailConfirmationToken { get; set; }
    
    public string Role { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public string PasswordHash { get; set; } = string.Empty;
    
    public string? ResetPasswordToken { get; set; }
    
    public DateTime? ResetPasswordTokenExpiry { get; set; }
}