using System.Net.Http.Headers;
using Mapster;
using Microsoft.AspNetCore.Http;
using UserManagement.Application.DTOs;
using UserManagement.Application.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Exceptions;
using UserManagement.Infrastructure.Interfaces;

namespace UserManagement.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserService(IUserRepository repository, IJwtTokenService jwtTokenService, HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _jwtTokenService = jwtTokenService;
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }
    
    public async Task<UserDto> GetById(int id)
    {
        var user = await _repository.GetByIdAsync(id)
                   ?? throw new NotFoundException($"User with id {id} not found");
        
        return user.Adapt<UserDto>();
    }

    public async Task<IEnumerable<UserDto>> GetAll()
    {
        var users = await _repository.GetAllAsync();
        return users.Adapt<List<UserDto>>();
    }

    public async Task<string> Create(UserRegisterDto userDto)
    {
        if (await _repository.GetByEmailAsync(userDto.Email) != null)
            throw new InvalidOperationException("User with this email already exists");
        
        var newAdmin = new User
        {
            Name = userDto.Name,
            Email = userDto.Email,
            Role = "Admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password),
            IsEmailConfirmed = true
        };

        await _repository.AddAsync(newAdmin);
        return _jwtTokenService.GenerateToken(newAdmin);
    }

    public async Task<bool> Update(UserUpdateDto userDto)
    {
        var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        if (string.IsNullOrEmpty(token))
        {
            throw new UnauthorizedAccessException("Token is missing");
        }
        
        var userId = _jwtTokenService.GetUserIdFromToken(token);
    
        if (userId == null)
        {
            throw new UnauthorizedAccessException("Invalid token");
        }
    
        var user = await _repository.GetByIdAsync(userId.Value) 
                   ?? throw new NotFoundException($"User with id {userId.Value} not found");

        user.Name = userDto.Name;
    
        await _repository.UpdateAsync(user);
        return true;
    }

    public async Task DeleteById(int id)
    {
        var user = await _repository.GetByIdAsync(id)
                   ?? throw new NotFoundException($"User with id {id} not found");
        await _repository.DeleteAsync(user.Id);
    }

    public async Task<bool> SetUserActiveStatus(int userId, bool isActive)
    {
        var user = await _repository.GetByIdAsync(userId) 
                   ?? throw new NotFoundException($"User with id {userId} not found");

        user.IsActive = isActive;
        await _repository.UpdateAsync(user);
        
        var requestUrl = $"http://productservice:5028/api/products/soft-delete/{userId}?isDeleted={!isActive}";

        using var request = new HttpRequestMessage(HttpMethod.Patch, requestUrl);
        
        var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        if (!string.IsNullOrEmpty(token) && _jwtTokenService.GetUserIdFromToken(token) != null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            throw new UnauthorizedAccessException("Invalid or missing token.");
        }

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to update user status. Status code: {response.StatusCode}");
        }

        return response.IsSuccessStatusCode;
    }
}