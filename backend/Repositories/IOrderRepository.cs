using backend.Models;

namespace backend.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(string id);
    Task<PaginatedResult<Order>> GetListAsync(string? status, string? searchField, string? searchKeyword, int page, int pageSize);
    Task<string> CreateAsync(Order order);
    Task RefundAsync(string id);
    Task CompleteAsync(string id);
    Task CancelAsync(string id);
    Task RestoreAsync(string id);
    Task SoftDeleteAsync(string id);
    Task<List<Order>> GetLatestAsync(int count);
    Task<int> GetTodayCountAsync();
    Task<decimal> GetTotalRevenueAsync();
    Task<decimal> GetRevenueByDateRangeAsync(DateTime from, DateTime to);
    Task<int> GetInProgressCountByUserAsync(int userId);
    Task<List<Order>> GetInProgressByUserAsync(int userId);
}
