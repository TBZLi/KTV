using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Services;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _dashboardService;

    public DashboardController(DashboardService dashboardService) => _dashboardService = dashboardService;

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var stats = await _dashboardService.GetStatsAsync(from, to);
        return Ok(stats);
    }

    [HttpGet("latest-orders")]
    public async Task<IActionResult> GetLatestOrders()
    {
        var orders = await _dashboardService.GetLatestOrdersAsync();
        return Ok(orders);
    }

    [HttpGet("top-songs")]
    public async Task<IActionResult> GetTopSongs()
    {
        var songs = await _dashboardService.GetTopSongsAsync();
        return Ok(songs);
    }
}
