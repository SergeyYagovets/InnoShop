using UserManagement.Domain.Entities;

namespace UserManagement.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
    string GenerateGuidToken();
    
    int? GetUserIdFromToken(string token);
}