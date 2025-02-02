using UserManagement.Application.DTOs;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.Interfaces;

public interface IUserService
{
    Task<UserDto> GetById(int id);
    Task<IEnumerable<UserDto>> GetAll();
    Task<string> Create(UserRegisterDto userDto);
    Task<bool> Update(UserUpdateDto userDto);
    Task DeleteById(int id);

    Task<bool> SetUserActiveStatus(int userId, bool isActive);
}