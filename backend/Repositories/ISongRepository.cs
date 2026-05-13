using backend.Models;

namespace backend.Repositories;

public interface ISongRepository
{
    Task<Song?> GetByIdAsync(int id);
    Task<PaginatedResult<Song>> GetListAsync(string? search, string? genre, int page, int pageSize);
    Task<int> CreateAsync(Song song);
    Task UpdateAsync(Song song);
    Task DeleteAsync(int id);
    Task<List<string>> GetGenresAsync();
    Task<SongStats> GetStatsAsync();
    Task IncrementPlayCountAsync(int songId);
}
