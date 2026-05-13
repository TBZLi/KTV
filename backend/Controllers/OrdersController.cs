using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.DTOs;
using backend.Services;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrdersController(OrderService orderService) => _orderService = orderService;

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] string? status, [FromQuery] string? searchField, [FromQuery] string? searchKeyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _orderService.GetListAsync(status, searchField, searchKeyword, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var order = await _orderService.GetByIdAsync(id);
        if (order == null) return NotFound();
        return Ok(order);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
    {
        var orderId = await _orderService.CreateAsync(request.UserId, request.RoomId, request.OrderType ?? "room", request.Hours, request.Amount);
        return Ok(new { id = orderId });
    }

    [HttpPost("{id}/refund")]
    public async Task<IActionResult> Refund(string id)
    {
        await _orderService.RefundAsync(id);
        return Ok();
    }

    [HttpPost("{id}/complete")]
    public async Task<IActionResult> Complete(string id)
    {
        await _orderService.CompleteAsync(id);
        return Ok();
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(string id)
    {
        await _orderService.CancelAsync(id);
        return Ok();
    }

    [HttpPost("{id}/restore")]
    public async Task<IActionResult> Restore(string id)
    {
        await _orderService.RestoreAsync(id);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _orderService.DeleteAsync(id);
        return Ok();
    }

    [HttpPost("song")]
    public async Task<IActionResult> OrderSong([FromBody] OrderSongRequest request)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        var queueId = await _orderService.OrderSongAsync(request.SongId, request.RoomId, userId);
        return Ok(new { id = queueId });
    }
}

public record CreateOrderRequest(int UserId, int RoomId, string? OrderType, decimal? Hours, decimal? Amount);
