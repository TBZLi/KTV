using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.DTOs;
using backend.Services;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SongsController : ControllerBase
{
    private readonly SongService _songService;

    public SongsController(SongService songService) => _songService = songService;

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] string? search, [FromQuery] string? genre, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _songService.GetListAsync(search, genre, page, pageSize);
        return Ok(result);
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _songService.GetStatsAsync();
        return Ok(stats);
    }

    [HttpGet("genres")]
    public async Task<IActionResult> GetGenres()
    {
        var genres = await _songService.GetGenresAsync();
        return Ok(genres);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSongRequest request)
    {
        var id = await _songService.CreateAsync(request.Title, request.Artist, request.Genre, request.Duration, request.CoverUrl, request.MediaUrl);
        return Ok(new { id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSongRequest request)
    {
        await _songService.UpdateAsync(id, request.Title, request.Artist, request.Genre, request.Duration, request.CoverUrl, request.MediaUrl, request.Status);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _songService.DeleteAsync(id);
        return Ok();
    }
}
