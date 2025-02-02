using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.DTOs;
using UserManagement.Application.Interfaces;

namespace UserManagement.Presentation.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("get-users")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
    {
        var users = await _userService.GetAll();
        return Ok(users);
    }

    [HttpGet("get-user{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<UserDto>> GetById([FromRoute] int id)
    {
        var user = await _userService.GetById(id);
        return Ok(user);
    }

    [HttpPost("create-admin")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult> Create([FromBody] UserRegisterDto userDto)
    {
        await _userService.Create(userDto);
        return Ok("Admin created successfully");
    }

    [HttpPut("change-name")]
    [Authorize(Policy = "AdminOrUser")]
    public async Task<IActionResult> Update([FromBody] UserUpdateDto user)
    {
        await _userService.Update(user);
        return Ok("Name updated successfully");
    }

    [HttpDelete("delete-user{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        await _userService.DeleteById(id);
        return Ok("User deleted successfully");
    }
    
    [HttpPatch("{userId}/activate")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> SetUserActiveStatus(int userId, [FromQuery] bool isActive)
    {
        await _userService.SetUserActiveStatus(userId, isActive);
        return Ok("User status updated successfully");
    }
}