using System.Collections.Concurrent;
using backend.DTOs;

namespace backend.Services;

public class PlaybackStateService
{
    private class RoomPlaybackState
    {
        public int CurrentQueueItemId { get; set; }
        public int SongId { get; set; }
        public string Title { get; set; } = "";
        public string Artist { get; set; } = "";
        public string CoverUrl { get; set; } = "";
        public string MediaUrl { get; set; } = "";
        public string LrcUrl { get; set; } = "";
        public int OrderedByUserId { get; set; }
        public string OrderedByName { get; set; } = "";
        public bool IsPlaying { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? PausedAt { get; set; }
        public double PausedPosition { get; set; }
        public int Duration { get; set; }
        public string PlayMode { get; set; } = "off";
    }

    private readonly ConcurrentDictionary<int, RoomPlaybackState> _states = new();

    public PlaybackStateDto GetState(int roomId)
    {
        if (!_states.TryGetValue(roomId, out var state))
        {
            return new PlaybackStateDto { HasTrack = false };
        }

        var currentTime = CalculateCurrentTime(state);
        return new PlaybackStateDto
        {
            HasTrack = true,
            CurrentQueueItemId = state.CurrentQueueItemId,
            SongId = state.SongId,
            Title = state.Title,
            Artist = state.Artist,
            CoverUrl = state.CoverUrl,
            MediaUrl = state.MediaUrl,
            LrcUrl = state.LrcUrl,
            OrderedByUserId = state.OrderedByUserId,
            OrderedByName = state.OrderedByName,
            IsPlaying = state.IsPlaying,
            CurrentTime = currentTime,
            Duration = state.Duration,
            PlayMode = state.PlayMode,
        };
    }

    public void Play(int roomId, int queueItemId, int songId, string title, string artist,
        string coverUrl, string mediaUrl, string lrcUrl, int orderedByUserId, string orderedByName,
        int duration, int userId)
    {
        if (orderedByUserId != userId)
            throw new UnauthorizedAccessException("只有当前歌曲的点歌人才能控制播放");

        _states[roomId] = new RoomPlaybackState
        {
            CurrentQueueItemId = queueItemId,
            SongId = songId,
            Title = title,
            Artist = artist,
            CoverUrl = coverUrl,
            MediaUrl = mediaUrl,
            LrcUrl = lrcUrl,
            OrderedByUserId = orderedByUserId,
            OrderedByName = orderedByName,
            IsPlaying = true,
            StartedAt = DateTime.UtcNow,
            Duration = duration,
            PlayMode = _states.TryGetValue(roomId, out var existing) ? existing.PlayMode : "off",
        };
    }

    public void Pause(int roomId, int userId)
    {
        if (!_states.TryGetValue(roomId, out var state) || !state.IsPlaying)
            return;

        if (state.OrderedByUserId != userId)
            throw new UnauthorizedAccessException("只有当前歌曲的点歌人才能控制播放");

        state.PausedPosition = CalculateCurrentTime(state);
        state.PausedAt = DateTime.UtcNow;
        state.IsPlaying = false;
    }

    public void Resume(int roomId, int userId)
    {
        if (!_states.TryGetValue(roomId, out var state) || state.IsPlaying)
            return;

        if (state.OrderedByUserId != userId)
            throw new UnauthorizedAccessException("只有当前歌曲的点歌人才能控制播放");

        state.StartedAt = DateTime.UtcNow.AddSeconds(-state.PausedPosition);
        state.PausedAt = null;
        state.IsPlaying = true;
    }

    public void Seek(int roomId, double position, int userId)
    {
        if (!_states.TryGetValue(roomId, out var state))
            return;

        if (state.OrderedByUserId != userId)
            throw new UnauthorizedAccessException("只有当前歌曲的点歌人才能控制播放");

        state.StartedAt = DateTime.UtcNow.AddSeconds(-position);
        if (state.IsPlaying == false)
        {
            state.PausedPosition = position;
            state.PausedAt = DateTime.UtcNow;
        }
    }

    public void SetPlayMode(int roomId, string mode, int userId)
    {
        if (!_states.TryGetValue(roomId, out var state))
            return;

        if (state.OrderedByUserId != userId)
            throw new UnauthorizedAccessException("只有当前歌曲的点歌人才能控制播放");

        state.PlayMode = mode;
    }

    public void Stop(int roomId)
    {
        _states.TryRemove(roomId, out _);
    }

    /// <summary>
    /// 检查歌曲是否已结束，若结束返回 true（调用方需处理自动切歌）
    /// </summary>
    public bool CheckTrackEnded(int roomId)
    {
        if (!_states.TryGetValue(roomId, out var state))
            return false;

        if (!state.IsPlaying)
            return false;

        var currentTime = CalculateCurrentTime(state);
        return currentTime >= state.Duration && state.Duration > 0;
    }

    private static double CalculateCurrentTime(RoomPlaybackState state)
    {
        if (!state.IsPlaying)
            return state.PausedPosition;

        if (state.StartedAt == null)
            return 0;

        var elapsed = (DateTime.UtcNow - state.StartedAt.Value).TotalSeconds;
        return Math.Max(0, elapsed);
    }
}
