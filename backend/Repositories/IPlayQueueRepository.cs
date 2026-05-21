using backend.Models;

namespace backend.Repositories;

public interface IPlayQueueRepository
{
    Task<List<PlayQueueItem>> GetByRoomIdAsync(int roomId);
    Task<int> AddAsync(PlayQueueItem item);
    Task RemoveAsync(int id);
    Task ReorderAsync(int queueId, int newOrder);
    Task ReorderBatchAsync(List<int> queueIds);
    Task MarkAsPlayedAsync(int id);
    Task MarkRoomPlayedAsync(int roomId);
}
