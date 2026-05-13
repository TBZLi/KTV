using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.DTOs;
using backend.Services;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService) => _authService = authService;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request.Username, request.Password);
        if (result == null) return Unauthorized(new { message = "用户名或密码错误" });
        return Ok(new { token = result.Value.Token, user = result.Value.User });
    }

    [HttpPost("logout")]
    public IActionResult Logout() => Ok();

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
