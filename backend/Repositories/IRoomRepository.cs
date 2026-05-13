using backend.Models;

namespace backend.Repositories;

public interface IRoomRepository
{
    Task<Room?> GetByIdAsync(int id);
    Task<PaginatedResult<Room>> GetListAsync(string? search, string? status, int page, int pageSize);
    Task UpdateStatusAsync(int id, string status);
    Task UpdateCurrentOrderIdAsync(int id, string? orderId);
    Task<int> GetCountAsync();
}
