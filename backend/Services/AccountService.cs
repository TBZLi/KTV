using backend.Models;
using backend.Repositories;

namespace backend.Services;

public class AccountService
{
    private readonly IUserRepository _userRepo;
    private readonly IOrderRepository _orderRepo;
    private readonly IRoomRepository _roomRepo;

    public AccountService(IUserRepository userRepo, IOrderRepository orderRepo, IRoomRepository roomRepo)
    {
        _userRepo = userRepo;
        _orderRepo = orderRepo;
        _roomRepo = roomRepo;
    }

    public async Task<PaginatedResult<User>> GetListAsync(string? search, string? status, int page, int pageSize)
    {
        return await _userRepo.GetListAsync(search, status, page, pageSize);
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _userRepo.GetByIdAsync(id);
    }

    public async Task<int> CreateAsync(string username, string password, string displayName, string? phone)
    {
        var existing = await _userRepo.GetByUsernameAsync(username);
        if (existing != null) throw new Exception("Username already exists");

        var user = new User
        {
            Username = username,
            PasswordHash = password, // TODO: hash password
            DisplayName = displayName,
            Phone = phone,
            Role = "user",
            Status = "active"
        };

        return await _userRepo.CreateAsync(user);
    }

    public async Task UpdateAsync(int id, string? displayName, string? phone, bool? isVip)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null) throw new Exception("User not found");

        if (displayName != null) user.DisplayName = displayName;
        if (phone != null) user.Phone = phone;
        if (isVip.HasValue) user.IsVip = isVip.Value;

        await _userRepo.UpdateAsync(user);
    }

    public async Task RechargeAsync(int id, decimal amount)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null) throw new Exception("User not found");
        await _userRepo.RechargeAsync(id, amount);
    }

    public async Task ToggleStatusAsync(int id)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null) throw new Exception("User not found");
        if (user.Role == "admin") throw new Exception("管理员账号不可禁用");

        user.Status = user.Status == "active" ? "disabled" : "active";
        await _userRepo.UpdateAsync(user);
    }

    public async Task<object> GetDisablePreviewAsync(int id)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null) throw new Exception("用户不存在");
        if (user.Role == "admin") throw new Exception("管理员账号不可禁用");
        if (user.Status == "disabled") throw new Exception("用户已被禁用");

        var inProgressCount = await _orderRepo.GetInProgressCountByUserAsync(id);
        return new { inProgressCount };
    }

    public async Task DisableWithAutoCancelAsync(int id)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null) throw new Exception("用户不存在");
        if (user.Role == "admin") throw new Exception("管理员账号不可禁用");
        if (user.Status == "disabled") throw new Exception("用户已被禁用");

        // Cancel all in-progress orders and refund
        var inProgressOrders = await _orderRepo.GetInProgressByUserAsync(id);
        foreach (var order in inProgressOrders)
        {
            await _userRepo.RechargeAsync(order.UserId, order.Amount);
            await _orderRepo.CancelAsync(order.Id);
            await _roomRepo.UpdateCurrentOrderIdAsync(order.RoomId, null);
            await _roomRepo.UpdateStatusAsync(order.RoomId, "idle");
        }

        // Disable user
        user.Status = "disabled";
        await _userRepo.UpdateAsync(user);
    }
}
