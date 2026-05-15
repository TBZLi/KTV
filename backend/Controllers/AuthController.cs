using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.DTOs;
using backend.Services;
using backend.Repositories;
using backend.Models;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly IUserRepository _userRepo;

    public AuthController(AuthService authService, IUserRepository userRepo)
    {
        _authService = authService;
        _userRepo = userRepo;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request.Username, request.Password);
        if (result == null) return Unauthorized(new { message = "用户名或密码错误" });
        return Ok(new { token = result.Value.Token, user = result.Value.User });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // At least one of username/phone/email must be provided
        if (string.IsNullOrEmpty(request.Username) && string.IsNullOrEmpty(request.Phone) && string.IsNullOrEmpty(request.Email))
            return BadRequest(new { message = "请至少填写一种注册方式（用户名/手机号/邮箱）" });

        if (string.IsNullOrEmpty(request.Password))
            return BadRequest(new { message = "密码不能为空" });

        if (string.IsNullOrEmpty(request.DisplayName))
            return BadRequest(new { message = "昵称不能为空" });

        // Check uniqueness
        if (!string.IsNullOrEmpty(request.Username))
        {
            var existing = await _userRepo.GetByUsernameAsync(request.Username);
            if (existing != null) return BadRequest(new { message = "用户名已存在" });
        }

        var user = new User
        {
            Username = request.Username ?? "",
            PasswordHash = request.Password, // Note: course project, not hashing
            DisplayName = request.DisplayName,
            Phone = request.Phone,
            Email = request.Email,
            Role = "user",
            Status = "active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var id = await _userRepo.CreateAsync(user);
        return Ok(new { id, message = "注册成功" });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        if (userId > 0)
            await _userRepo.UpdateLastActiveAtAsync(userId, clear: true);
        return Ok();
    }

    [HttpGet("me")]
    public IActionResult GetMe() => Ok(new { message = "TODO: get from JWT claims" });

    [HttpPost("verify-password")]
    [Authorize]
    public async Task<IActionResult> VerifyPassword([FromBody] VerifyPasswordRequest request)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        var valid = await _authService.VerifyPasswordAsync(userId, request.Password);
        if (!valid) return Unauthorized(new { message = "密码错误" });
        return Ok();
    }
}
