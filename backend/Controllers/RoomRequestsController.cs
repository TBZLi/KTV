using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Services;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoomRequestsController : ControllerBase
{
    private readonly RoomRequestService _requestService;

    public RoomRequestsController(RoomRequestService requestService) => _requestService = requestService;

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _requestService.GetListAsync(status, page, pageSize);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create()
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        var id = await _requestService.CreateAsync(userId);
        return Ok(new { id, message = "申请已提交，等待管理员审批" });
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var adminId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        try
        {
            var room = await _requestService.ApproveAsync(id, adminId);
            return Ok(new { roomCode = room.RoomCode, roomId = room.Id, message = "已通过，房间已创建" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(int id)
    {
        var adminId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        try
        {
            await _requestService.RejectAsync(id, adminId);
            return Ok(new { message = "已拒绝" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("pending-count")]
    public async Task<IActionResult> GetPendingCount()
    {
        var count = await _requestService.GetPendingCountAsync();
        return Ok(new { count });
    }
}
