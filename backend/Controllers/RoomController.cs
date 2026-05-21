using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.DTOs;
using backend.Repositories;
using backend.Services;
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
    private readonly RoomService _roomService;
    private readonly PlaybackStateService _playback;
    private readonly string _connStr;

    public RoomController(IPlayQueueRepository queueRepo, RoomService roomService, PlaybackStateService playback, string connStr)
    {
        _queueRepo = queueRepo;
        _roomService = roomService;
        _playback = playback;
        _connStr = connStr;
    }

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrent([FromQuery] int? roomId)
    {
        using var conn = new SqlConnection(_connStr);

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        Room? room = null;

        // If roomId provided, verify user is actually in that room
        if (roomId.HasValue && roomId.Value > 0)
        {
            var isInRoom = await conn.ExecuteScalarAsync<bool>(
                "SELECT COUNT(1) FROM RoomUsers WHERE RoomId = @RoomId AND UserId = @UserId",
                new { RoomId = roomId.Value, UserId = userId });
            if (isInRoom)
            {
                room = await conn.QuerySingleOrDefaultAsync<Room>(
                    "SELECT * FROM Rooms WHERE Id = @Id AND Status != 'closed'",
                    new { Id = roomId.Value });
            }
        }

        // Fallback: find any room the user is in
        if (room == null)
        {
            room = await conn.QuerySingleOrDefaultAsync<Room>(
                @"SELECT r.* FROM Rooms r
                  INNER JOIN RoomUsers ru ON ru.RoomId = r.Id
                  WHERE ru.UserId = @UserId AND r.Status != 'closed'",
                new { UserId = userId });
        }

        if (room == null) return Ok(new { roomCode = "N/A", roomId = 0, songsQueued = 0, onlineUsers = 0 });

        // Sync user count to fix stale entries
        await _roomService.SyncUserCountAsync(room.Id);
        room = await _roomService.GetByIdAsync(room.Id);

        var queueCount = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM PlayQueue WHERE RoomId = @RoomId AND Status = 'queued'",
            new { RoomId = room!.Id });

        return Ok(new
        {
            roomId = room.Id,
            roomCode = room.RoomCode,
            songsQueued = queueCount,
            onlineUsers = room.CurrentUsers
        });
    }

    [HttpGet("queue")]
    public async Task<IActionResult> GetQueue()
    {
        using var conn = new SqlConnection(_connStr);
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        // Only return queue for a room the user is actually in
        var roomId = await conn.ExecuteScalarAsync<int>(
            @"SELECT TOP 1 ru.RoomId FROM RoomUsers ru
              INNER JOIN Rooms r ON r.Id = ru.RoomId AND r.Status != 'closed'
              WHERE ru.UserId = @UserId",
            new { UserId = userId });
        if (roomId == 0)
            return Ok(Array.Empty<object>());

        var queue = await _queueRepo.GetByRoomIdAsync(roomId);
        return Ok(queue);
    }

    [HttpPost("queue")]
    public async Task<IActionResult> AddToQueue([FromBody] OrderSongRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        using var conn = new SqlConnection(_connStr);

        // Find the room the user is actually in
        var roomId = await conn.ExecuteScalarAsync<int>(
            @"SELECT TOP 1 ru.RoomId FROM RoomUsers ru
              INNER JOIN Rooms r ON r.Id = ru.RoomId AND r.Status != 'closed'
              WHERE ru.UserId = @UserId",
            new { UserId = userId });
        if (roomId <= 0) return BadRequest(new { message = "请先加入房间再点歌" });

        var songExists = await conn.ExecuteScalarAsync<bool>(
            "SELECT COUNT(1) FROM Songs WHERE Id = @Id AND Status = 'active'",
            new { Id = request.SongId });
        if (!songExists) return BadRequest(new { message = "歌曲不存在或已下架" });

        // Check if song is already in queue (not yet played)
        var alreadyQueued = await conn.ExecuteScalarAsync<bool>(
            "SELECT COUNT(1) FROM PlayQueue WHERE RoomId = @RoomId AND SongId = @SongId AND Status = 'queued'",
            new { RoomId = roomId, SongId = request.SongId });
        if (alreadyQueued) return BadRequest(new { message = "这首歌已经在播放列表中了" });

        var item = new PlayQueueItem
        {
            RoomId = roomId,
            SongId = request.SongId,
            OrderedByUserId = userId,
        };
        var id = await _queueRepo.AddAsync(item);
        return Ok(new { id });
    }

    [HttpPost("queue/reorder")]
    public async Task<IActionResult> ReorderQueue([FromBody] ReorderQueueRequest request)
    {
        await _queueRepo.ReorderAsync(request.QueueId, request.NewOrder);
        return Ok();
    }

    [HttpPost("queue/reorder-batch")]
    public async Task<IActionResult> ReorderBatch([FromBody] ReorderBatchRequest request)
    {
        if (request.QueueIds == null || request.QueueIds.Count == 0)
            return BadRequest(new { message = "队列ID列表不能为空" });
        await _queueRepo.ReorderBatchAsync(request.QueueIds);
        return Ok();
    }

    [HttpDelete("queue/{id}")]
    public async Task<IActionResult> RemoveFromQueue(int id)
    {
        await _queueRepo.RemoveAsync(id);
        return Ok();
    }

    [HttpGet("{roomId}/users")]
    public async Task<IActionResult> GetRoomUsers(int roomId)
    {
        var users = await _roomService.GetRoomUsersAsync(roomId);
        return Ok(users);
    }

    // ── Playback Sync Endpoints ──

    private async Task<int> GetUserRoomIdAsync(int userId)
    {
        using var conn = new SqlConnection(_connStr);
        return await conn.ExecuteScalarAsync<int>(
            @"SELECT TOP 1 ru.RoomId FROM RoomUsers ru
              INNER JOIN Rooms r ON r.Id = ru.RoomId AND r.Status != 'closed'
              WHERE ru.UserId = @UserId",
            new { UserId = userId });
    }

    [HttpGet("playback")]
    public async Task<IActionResult> GetPlaybackState()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var roomId = await GetUserRoomIdAsync(userId);
        if (roomId <= 0) return Ok(new { hasTrack = false });

        // Check if track ended and auto-advance
        if (_playback.CheckTrackEnded(roomId))
        {
            var state = _playback.GetState(roomId);
            if (state.HasTrack)
            {
                await _queueRepo.MarkAsPlayedAsync(state.CurrentQueueItemId);
                await AdvanceToNextTrack(roomId, state.PlayMode);
            }
        }

        return Ok(_playback.GetState(roomId));
    }

    [HttpPost("playback/play")]
    public async Task<IActionResult> PlaybackPlay([FromBody] PlayRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var roomId = await GetUserRoomIdAsync(userId);
        if (roomId <= 0) return BadRequest(new { message = "未在房间中" });

        var queue = await _queueRepo.GetByRoomIdAsync(roomId);
        var item = queue.FirstOrDefault(q => q.Id == request.QueueItemId);
        if (item == null) return BadRequest(new { message = "队列项不存在" });

        var duration = await GetSongDurationAsync(item.SongId);
        _playback.Play(roomId, item.Id, item.SongId, item.SongTitle, item.Artist,
            item.CoverUrl ?? "", item.MediaUrl ?? "", item.LrcUrl ?? "",
            item.OrderedByUserId, item.OrderedBy, duration, userId);
        return Ok();
    }

    [HttpPost("playback/pause")]
    public async Task<IActionResult> PlaybackPause()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var roomId = await GetUserRoomIdAsync(userId);
        if (roomId <= 0) return BadRequest(new { message = "未在房间中" });

        try { _playback.Pause(roomId, userId); return Ok(); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
    }

    [HttpPost("playback/resume")]
    public async Task<IActionResult> PlaybackResume()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var roomId = await GetUserRoomIdAsync(userId);
        if (roomId <= 0) return BadRequest(new { message = "未在房间中" });

        try { _playback.Resume(roomId, userId); return Ok(); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
    }

    [HttpPost("playback/seek")]
    public async Task<IActionResult> PlaybackSeek([FromBody] SeekRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var roomId = await GetUserRoomIdAsync(userId);
        if (roomId <= 0) return BadRequest(new { message = "未在房间中" });

        try { _playback.Seek(roomId, request.Position, userId); return Ok(); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
    }

    [HttpPost("playback/next")]
    public async Task<IActionResult> PlaybackNext()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var roomId = await GetUserRoomIdAsync(userId);
        if (roomId <= 0) return BadRequest(new { message = "未在房间中" });

        var state = _playback.GetState(roomId);
        if (!state.HasTrack) return BadRequest(new { message = "没有播放歌曲" });
        if (state.OrderedByUserId != userId) return StatusCode(403, new { message = "只有当前歌曲的点歌人才能控制播放" });

        await _queueRepo.MarkAsPlayedAsync(state.CurrentQueueItemId);
        await AdvanceToNextTrack(roomId, state.PlayMode);
        return Ok();
    }

    [HttpPost("playback/prev")]
    public async Task<IActionResult> PlaybackPrev()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var roomId = await GetUserRoomIdAsync(userId);
        if (roomId <= 0) return BadRequest(new { message = "未在房间中" });

        var state = _playback.GetState(roomId);
        if (!state.HasTrack) return BadRequest(new { message = "没有播放歌曲" });
        if (state.OrderedByUserId != userId) return StatusCode(403, new { message = "只有当前歌曲的点歌人才能控制播放" });

        // Get queue and find previous item
        var queue = await _queueRepo.GetByRoomIdAsync(roomId);
        var currentIdx = queue.FindIndex(q => q.Id == state.CurrentQueueItemId);
        if (currentIdx <= 0) return BadRequest(new { message = "没有上一首" });

        var prev = queue[currentIdx - 1];
        var duration = await GetSongDurationAsync(prev.SongId);
        _playback.Play(roomId, prev.Id, prev.SongId, prev.SongTitle, prev.Artist,
            prev.CoverUrl ?? "", prev.MediaUrl ?? "", prev.LrcUrl ?? "",
            prev.OrderedByUserId, prev.OrderedBy, duration, userId);
        return Ok();
    }

    [HttpPost("playback/mode")]
    public async Task<IActionResult> PlaybackMode([FromBody] PlayModeRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var roomId = await GetUserRoomIdAsync(userId);
        if (roomId <= 0) return BadRequest(new { message = "未在房间中" });

        try { _playback.SetPlayMode(roomId, request.Mode, userId); return Ok(); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
    }

    private async Task AdvanceToNextTrack(int roomId, string playMode)
    {
        var queue = await _queueRepo.GetByRoomIdAsync(roomId);

        if (queue.Count == 0)
        {
            if (playMode == "repeat-one")
            {
                // repeat-one with no queue means the track was already marked played,
                // but there's nothing to repeat to — stop
                _playback.Stop(roomId);
                return;
            }
            _playback.Stop(roomId);
            return;
        }

        // queue is ordered by SortOrder, first item is the next track
        var next = queue[0];
        var duration = await GetSongDurationAsync(next.SongId);
        _playback.Play(roomId, next.Id, next.SongId, next.SongTitle, next.Artist,
            next.CoverUrl ?? "", next.MediaUrl ?? "", next.LrcUrl ?? "",
            next.OrderedByUserId, next.OrderedBy, duration, next.OrderedByUserId);
    }

    private async Task<int> GetSongDurationAsync(int songId)
    {
        using var conn = new SqlConnection(_connStr);
        return await conn.ExecuteScalarAsync<int>(
            "SELECT ISNULL(Duration, 0) FROM Songs WHERE Id = @Id",
            new { Id = songId });
    }
}
