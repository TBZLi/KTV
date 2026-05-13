using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.DTOs;
using backend.Repositories;
using backend.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoomController : ControllerBase
{
    private readonly IPlayQueueRepository _queueRepo;
    private readonly string _connStr;

    public RoomController(IPlayQueueRepository queueRepo, string connStr)
    {
        _queueRepo = queueRepo;
        _connStr = connStr;
    }

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrent()
    {
        using var conn = new SqlConnection(_connStr);
        // Prefer rooms that have queued songs, then fall back to any in_use room
        var room = await conn.QuerySingleOrDefaultAsync<Room>(
            @"SELECT TOP 1 r.* FROM Rooms r
              INNER JOIN PlayQueue pq ON pq.RoomId = r.Id AND pq.Status = 'queued'
              WHERE r.Status = 'in_use'
              GROUP BY r.Id, r.RoomNumber, r.RoomType, r.Capacity, r.HourlyRate, r.Status, r.CurrentOrderId, r.CreatedAt, r.UpdatedAt
              ORDER BY r.Id")
            ?? await conn.QuerySingleOrDefaultAsync<Room>(
            "SELECT TOP 1 * FROM Rooms WHERE Status = 'in_use' ORDER BY Id")
            ?? await conn.QuerySingleOrDefaultAsync<Room>(
            "SELECT TOP 1 * FROM Rooms ORDER BY Id");

        if (room == null) return Ok(new { roomNumber = "N/A", roomType = "N/A", songsQueued = 0 });

        var queueCount = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM PlayQueue WHERE RoomId = @RoomId AND Status = 'queued'",
            new { RoomId = room.Id });

        return Ok(new
        {
            roomNumber = room.RoomNumber,
            roomType = room.RoomType,
            songsQueued = queueCount
        });
    }

    [HttpGet("queue")]
    public async Task<IActionResult> GetQueue()
    {
        using var conn = new SqlConnection(_connStr);
        // Prefer rooms that have queued songs
        var roomId = await conn.ExecuteScalarAsync<int>(
            @"SELECT TOP 1 r.Id FROM Rooms r
              INNER JOIN PlayQueue pq ON pq.RoomId = r.Id AND pq.Status = 'queued'
              WHERE r.Status = 'in_use'
              ORDER BY r.Id");
        if (roomId == 0)
            roomId = await conn.ExecuteScalarAsync<int>(
                "SELECT TOP 1 Id FROM Rooms WHERE Status = 'in_use' ORDER BY Id");
        if (roomId == 0)
            roomId = await conn.ExecuteScalarAsync<int>("SELECT TOP 1 Id FROM Rooms ORDER BY Id");

        var queue = await _queueRepo.GetByRoomIdAsync(roomId);
        return Ok(queue);
    }

    [HttpPost("queue/reorder")]
    public async Task<IActionResult> ReorderQueue([FromBody] ReorderQueueRequest request)
    {
        await _queueRepo.ReorderAsync(request.QueueId, request.NewOrder);
        return Ok();
    }

    [HttpDelete("queue/{id}")]
    public async Task<IActionResult> RemoveFromQueue(int id)
    {
        await _queueRepo.RemoveAsync(id);
        return Ok();
    }
}
