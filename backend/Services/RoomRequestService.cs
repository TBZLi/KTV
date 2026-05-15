using backend.Models;
using backend.Repositories;

namespace backend.Services;

public class RoomRequestService
{
    private readonly IRoomRequestRepository _requestRepo;
    private readonly IRoomRepository _roomRepo;

    public RoomRequestService(IRoomRequestRepository requestRepo, IRoomRepository roomRepo)
    {
        _requestRepo = requestRepo;
        _roomRepo = roomRepo;
    }

    public async Task<PaginatedResult<RoomRequest>> GetListAsync(string? status, int page, int pageSize)
    {
        return await _requestRepo.GetListAsync(status, page, pageSize);
    }

    public async Task<int> CreateAsync(int userId)
    {
        var request = new RoomRequest { UserId = userId };
        return await _requestRepo.CreateAsync(request);
    }

    public async Task<Room> ApproveAsync(int requestId, int adminId)
    {
        var request = await _requestRepo.GetByIdAsync(requestId);
        if (request == null) throw new Exception("申请不存在");
        if (request.Status != "pending") throw new Exception("该申请已处理");

        // Generate 6-char room code
        var roomCode = GenerateRoomCode();
        var roomId = await _roomRepo.CreateAsync(roomCode, request.UserId);
        await _requestRepo.UpdateStatusAsync(requestId, "approved", roomId, adminId);

        return (await _roomRepo.GetByIdAsync(roomId))!;
    }

    public async Task RejectAsync(int requestId, int adminId)
    {
        var request = await _requestRepo.GetByIdAsync(requestId);
        if (request == null) throw new Exception("申请不存在");
        if (request.Status != "pending") throw new Exception("该申请已处理");

        await _requestRepo.UpdateStatusAsync(requestId, "rejected", null, adminId);
    }

    public async Task<int> GetPendingCountAsync()
    {
        return await _requestRepo.GetPendingCountAsync();
    }

    private static string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var random = new Random();
        return new string(Enumerable.Range(0, 6).Select(_ => chars[random.Next(chars.Length)]).ToArray());
    }
}
