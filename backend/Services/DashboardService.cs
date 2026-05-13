using backend.Models;
using backend.Repositories;

namespace backend.Services;

public class DashboardService
{
    private readonly IRoomRepository _roomRepo;
    private readonly IOrderRepository _orderRepo;
    private readonly IUserRepository _userRepo;
    private readonly ISongRepository _songRepo;

    public DashboardService(
        IRoomRepository roomRepo,
        IOrderRepository orderRepo,
        IUserRepository userRepo,
        ISongRepository songRepo)
    {
        _roomRepo = roomRepo;
        _orderRepo = orderRepo;
        _userRepo = userRepo;
        _songRepo = songRepo;
    }

    public async Task<DashboardStats> GetStatsAsync(DateTime? from = null, DateTime? to = null)
    {
        var totalRooms = await _roomRepo.GetCountAsync();
        var todayOrders = await _orderRepo.GetTodayCountAsync();
        var activeUsers = await _userRepo.GetActiveCountAsync();

        decimal totalRevenue;
        if (from.HasValue && to.HasValue)
        {
            totalRevenue = await _orderRepo.GetRevenueByDateRangeAsync(from.Value, to.Value);
        }
        else
        {
            totalRevenue = await _orderRepo.GetTotalRevenueAsync();
        }

        return new DashboardStats
        {
            TotalRooms = totalRooms,
            TodayOrders = todayOrders,
            TotalRevenue = totalRevenue,
            ActiveUsers = activeUsers
        };
    }

    public async Task<List<Order>> GetLatestOrdersAsync()
    {
        return await _orderRepo.GetLatestAsync(10);
    }

    public async Task<List<Song>> GetTopSongsAsync()
    {
        var result = await _songRepo.GetListAsync(null, null, 1, 10);
        return result.Items;
    }
}
