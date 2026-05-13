using backend.Models;
using backend.Repositories;

namespace backend.Services;

public class SongService
{
    private readonly ISongRepository _songRepo;

    public SongService(ISongRepository songRepo)
    {
        _songRepo = songRepo;
    }

    public async Task<PaginatedResult<Song>> GetListAsync(string? search, string? genre, int page, int pageSize)
    {
        return await _songRepo.GetListAsync(search, genre, page, pageSize);
    }

    public async Task<Song?> GetByIdAsync(int id)
    {
        return await _songRepo.GetByIdAsync(id);
    }

    public async Task<int> CreateAsync(string title, string artist, string genre, int duration, string? coverUrl, string? mediaUrl)
    {
        var song = new Song
        {
            Title = title,
            Artist = artist,
            Genre = genre,
            Duration = duration,
            CoverUrl = coverUrl,
            MediaUrl = mediaUrl
        };
        return await _songRepo.CreateAsync(song);
    }

    public async Task UpdateAsync(int id, string? title, string? artist, string? genre, int? duration, string? coverUrl, string? mediaUrl, string? status)
    {
        var song = await _songRepo.GetByIdAsync(id);
        if (song == null) throw new Exception("Song not found");

        if (title != null) song.Title = title;
        if (artist != null) song.Artist = artist;
        if (genre != null) song.Genre = genre;
        if (duration.HasValue) song.Duration = duration.Value;
        if (coverUrl != null) song.CoverUrl = coverUrl;
        if (mediaUrl != null) song.MediaUrl = mediaUrl;
        if (status != null) song.Status = status;

        await _songRepo.UpdateAsync(song);
    }

    public async Task DeleteAsync(int id)
    {
        await _songRepo.DeleteAsync(id);
    }

    public async Task<List<string>> GetGenresAsync()
    {
        return await _songRepo.GetGenresAsync();
    }

    public async Task<SongStats> GetStatsAsync()
    {
        return await _songRepo.GetStatsAsync();
    }
}
