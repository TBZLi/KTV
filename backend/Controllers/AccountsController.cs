using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.DTOs;
using backend.Services;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly AccountService _accountService;

    public AccountsController(AccountService accountService) => _accountService = accountService;

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] string? search, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _accountService.GetListAsync(search, status, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _accountService.GetByIdAsync(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAccountRequest request)
    {
        var id = await _accountService.CreateAsync(request.Username, request.Password, request.DisplayName, request.Phone);
        return Ok(new { id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAccountRequest request)
    {
        await _accountService.UpdateAsync(id, request.DisplayName, request.Phone, request.AvatarUrl, request.IsVip);
        return Ok();
    }

    [HttpPost("{id}/recharge")]
    public async Task<IActionResult> Recharge(int id, [FromBody] RechargeRequest request)
    {
        await _accountService.RechargeAsync(id, request.Amount);
        return Ok();
    }

    [HttpPut("{id}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        await _accountService.ToggleStatusAsync(id);
        return Ok();
    }

    [HttpGet("{id}/disable-preview")]
    public async Task<IActionResult> GetDisablePreview(int id)
    {
        var result = await _accountService.GetDisablePreviewAsync(id);
        return Ok(result);
    }

    [HttpPost("{id}/disable")]
    public async Task<IActionResult> Disable(int id)
    {
        await _accountService.DisableWithAutoCancelAsync(id);
        return Ok();
    }
}
