using backend.Models;
using backend.Repositories;

namespace backend.Services;

public class OrderService
{
    private readonly IOrderRepository _orderRepo;
    private readonly IRoomRepository _roomRepo;
    private readonly IPlayQueueRepository _queueRepo;
    private readonly ISongRepository _songRepo;
    private readonly IUserRepository _userRepo;
    private readonly ISettingsRepository _settingsRepo;
    private readonly IHolidayRepository _holidayRepo;

    public OrderService(
        IOrderRepository orderRepo,
        IRoomRepository roomRepo,
        IPlayQueueRepository queueRepo,
        ISongRepository songRepo,
        IUserRepository userRepo,
        ISettingsRepository settingsRepo,
        IHolidayRepository holidayRepo)
    {
        _orderRepo = orderRepo;
        _roomRepo = roomRepo;
        _queueRepo = queueRepo;
        _songRepo = songRepo;
        _userRepo = userRepo;
        _settingsRepo = settingsRepo;
        _holidayRepo = holidayRepo;
    }

    public async Task<PaginatedResult<Order>> GetListAsync(string? status, string? searchField, string? searchKeyword, int page, int pageSize)
    {
        return await _orderRepo.GetListAsync(status, searchField, searchKeyword, page, pageSize);
    }

    public async Task<Order?> GetByIdAsync(string id)
    {
        return await _orderRepo.GetByIdAsync(id);
    }

    public async Task<string> CreateAsync(int userId, int roomId, string orderType, decimal? hours, decimal? amountOverride)
    {
        // Validate user exists
        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null) throw new Exception("用户不存在");
        if (user.Status != "active") throw new Exception("用户账户已被禁用");

        // Validate room
        var room = await _roomRepo.GetByIdAsync(roomId);
        if (room == null) throw new Exception("房间不存在");
        if (room.Status == "in_use") throw new Exception($"房间 {room.RoomNumber} 正在使用中，无法新建订单");

        // VIP room restriction
        if (room.RoomType == "VIP" && !user.IsVip)
            throw new Exception("VIP 包厢仅限 VIP 用户使用");

        // One active order per user
        var existingOrders = await _orderRepo.GetInProgressByUserAsync(userId);
        if (existingOrders.Count > 0)
            throw new Exception("该用户已有进行中订单，请先完成或取消当前订单");

        // Calculate amount: override > hours * rate (with holiday multiplier)
        decimal amount;
        if (amountOverride.HasValue)
        {
            amount = amountOverride.Value;
        }
        else if (hours.HasValue)
        {
            var settings = await _settingsRepo.GetAllAsync();
            var hourlyRate = (settings.TryGetValue("baseHourlyRate", out var rateStr1) && decimal.TryParse(rateStr1, out var rate1))
                || (settings.TryGetValue("base_hourly_rate", out var rateStr2) && decimal.TryParse(rateStr2, out rate1))
                ? rate1 : 120m;

            // Room type multiplier (always applied)
            var rtKey = room.RoomType.ToLower() == "vip" ? "room_type_multiplier_vip"
                : room.RoomType.ToLower() == "medium" ? "room_type_multiplier_medium"
                : "room_type_multiplier_small";
            var roomTypeMultiplier = settings.TryGetValue(rtKey, out var rtStr) && decimal.TryParse(rtStr, out var rtVal)
                ? rtVal : (room.RoomType == "VIP" ? 1.5m : room.RoomType == "Medium" ? 1.3m : 1.0m);

            // Apply holiday multiplier if enabled and active
            decimal holidayMultiplier = 1.0m;
            if (settings.TryGetValue("holidayPricingEnabled", out var hpEnabled) && hpEnabled == "true"
                || settings.TryGetValue("holiday_pricing_enabled", out hpEnabled) && hpEnabled == "true")
            {
                var activeHolidays = await _holidayRepo.GetActiveHolidaysAsync(DateTime.UtcNow);
                if (activeHolidays.Count > 0)
                {
                    holidayMultiplier = room.RoomType switch
                    {
                        "VIP" => activeHolidays.Max(h => h.VipMultiplier),
                        "Medium" => activeHolidays.Max(h => h.MediumMultiplier),
                        "Small" => activeHolidays.Max(h => h.SmallMultiplier),
                        _ => 1.0m
                    };
                }
            }

            amount = hours.Value * hourlyRate * roomTypeMultiplier * holidayMultiplier;
            amount = Math.Round(amount, 2);
        }
        else
        {
            throw new Exception("请指定使用时长或自定义金额");
        }

        if (amount <= 0) throw new Exception("金额必须大于 0");

        // Atomically deduct balance (checks balance >= amount in same UPDATE)
        var deducted = await _userRepo.TryDeductBalanceAsync(userId, amount);
        if (!deducted)
        {
            var balance = await _userRepo.GetBalanceAsync(userId);
            throw new Exception($"账户余额不足，当前余额: {balance:F2}，需要: {amount:F2}");
        }

        // Create order
        var order = new Order
        {
            UserId = userId,
            RoomId = roomId,
            OrderType = orderType,
            Amount = amount,
            Status = "in_progress",
            StartTime = DateTime.UtcNow
        };

        var orderId = await _orderRepo.CreateAsync(order);

        // Set room to in_use and bind order
        await _roomRepo.UpdateCurrentOrderIdAsync(roomId, orderId);
        await _roomRepo.UpdateStatusAsync(roomId, "in_use");

        return orderId;
    }

    public async Task RefundAsync(string id)
    {
        var order = await _orderRepo.GetByIdAsync(id);
        if (order == null) throw new Exception("订单不存在");
        if (order.Status != "in_progress") throw new Exception($"订单状态为 {order.Status}，无法退款");

        // Refund balance to user
        await _userRepo.RechargeAsync(order.UserId, order.Amount);

        // Update order status
        await _orderRepo.RefundAsync(id);

        // Set room back to idle and clear order binding
        await _roomRepo.UpdateCurrentOrderIdAsync(order.RoomId, null);
        await _roomRepo.UpdateStatusAsync(order.RoomId, "idle");
    }

    public async Task CompleteAsync(string id)
    {
        var order = await _orderRepo.GetByIdAsync(id);
        if (order == null) throw new Exception("订单不存在");
        if (order.Status != "in_progress") throw new Exception($"订单状态为 {order.Status}，无法完成");

        await _orderRepo.CompleteAsync(id);

        // Room goes to cleaning, clear order binding
        await _roomRepo.UpdateCurrentOrderIdAsync(order.RoomId, null);
        await _roomRepo.UpdateStatusAsync(order.RoomId, "cleaning");
    }

    public async Task CancelAsync(string id)
    {
        var order = await _orderRepo.GetByIdAsync(id);
        if (order == null) throw new Exception("订单不存在");
        if (order.Status != "in_progress") throw new Exception($"订单状态为 {order.Status}，无法取消");

        // Refund balance to user
        await _userRepo.RechargeAsync(order.UserId, order.Amount);

        await _orderRepo.CancelAsync(id);

        // Room goes to idle, clear order binding
        await _roomRepo.UpdateCurrentOrderIdAsync(order.RoomId, null);
        await _roomRepo.UpdateStatusAsync(order.RoomId, "idle");
    }

    public async Task RestoreAsync(string id)
    {
        var order = await _orderRepo.GetByIdAsync(id);
        if (order == null) throw new Exception("订单不存在");
        if (order.Status != "cancelled") throw new Exception($"订单状态为 {order.Status}，仅已取消订单可恢复");

        // Check user exists and is active
        var user = await _userRepo.GetByIdAsync(order.UserId);
        if (user == null) throw new Exception("用户不存在");
        if (user.Status != "active") throw new Exception("用户账户已被禁用，无法恢复订单");

        // Check balance
        var balance = await _userRepo.GetBalanceAsync(order.UserId);
        if (balance < order.Amount) throw new Exception($"账户余额不足，当前余额: {balance:F2}，需要: {order.Amount:F2}");

        // Check room is available
        var room = await _roomRepo.GetByIdAsync(order.RoomId);
        if (room == null) throw new Exception("房间不存在");
        if (room.Status == "in_use") throw new Exception($"房间 {room.RoomNumber} 正在使用中，无法恢复订单");

        // Deduct balance
        var deducted = await _userRepo.TryDeductBalanceAsync(order.UserId, order.Amount);
        if (!deducted) throw new Exception("扣款失败");

        // Restore order
        await _orderRepo.RestoreAsync(id);

        // Rebind room
        await _roomRepo.UpdateCurrentOrderIdAsync(order.RoomId, id);
        await _roomRepo.UpdateStatusAsync(order.RoomId, "in_use");
    }

    public async Task DeleteAsync(string id)
    {
        var order = await _orderRepo.GetByIdAsync(id);
        if (order == null) throw new Exception("订单不存在");
        if (order.Status == "in_progress") throw new Exception("进行中的订单无法删除，请先完成、取消或退款");
        if (order.IsDeleted) throw new Exception("订单已删除");

        // Refund if the order was charged (completed status means money was taken)
        if (order.Status == "completed")
        {
            await _userRepo.RechargeAsync(order.UserId, order.Amount);
        }

        await _orderRepo.SoftDeleteAsync(id);
    }

    public async Task<int> OrderSongAsync(int songId, int roomId, int userId)
    {
        var song = await _songRepo.GetByIdAsync(songId);
        if (song == null) throw new Exception("Song not found");

        var item = new PlayQueueItem
        {
            RoomId = roomId,
            SongId = songId,
            OrderedByUserId = userId
        };

        var queueId = await _queueRepo.AddAsync(item);
        await _songRepo.IncrementPlayCountAsync(songId);
        return queueId;
    }
}
