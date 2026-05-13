using backend.Models;
using backend.Repositories;

namespace backend.Services;

public class RoomService
{
    private readonly IRoomRepository _roomRepo;
    private readonly IOrderRepository _orderRepo;

    public RoomService(IRoomRepository roomRepo, IOrderRepository orderRepo)
    {
        _roomRepo = roomRepo;
        _orderRepo = orderRepo;
    }

    public async Task<PaginatedResult<Room>> GetListAsync(string? search, string? status, int page, int pageSize)
    {
        return await _roomRepo.GetListAsync(search, status, page, pageSize);
    }

    public async Task<Room?> GetByIdAsync(int id)
    {
        return await _roomRepo.GetByIdAsync(id);
    }

    public async Task UpdateStatusAsync(int id, string status)
    {
        var room = await _roomRepo.GetByIdAsync(id);
        if (room == null) throw new Exception("Room not found");
        await _roomRepo.UpdateStatusAsync(id, status);
    }

    public async Task EndSessionAsync(int id)
    {
        var room = await _roomRepo.GetByIdAsync(id);
        if (room == null) throw new Exception("Room not found");

        // Refund current order if exists
        if (!string.IsNullOrEmpty(room.CurrentOrderId))
        {
            var order = await _orderRepo.GetByIdAsync(room.CurrentOrderId);
            if (order != null && order.Status == "in_progress")
            {
                await _orderRepo.RefundAsync(room.CurrentOrderId);
            }
        }

        await _roomRepo.UpdateCurrentOrderIdAsync(id, null);
        await _roomRepo.UpdateStatusAsync(id, "idle");
    }
}
