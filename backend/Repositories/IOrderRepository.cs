using System.Data;
using backend.Models;

namespace backend.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(string id);
    Task<PaginatedResult<Order>> GetListAsync(string? status, string? searchField, string? searchKeyword, int page, int pageSize);
    Task<string> CreateAsync(Order order, IDbTransaction? tran = null);
    Task RefundAsync(string id, IDbTransaction? tran = null);
    Task CompleteAsync(string id, IDbTransaction? tran = null);
    Task CancelAsync(string id, IDbTransaction? tran = null);
    Task RestoreAsync(string id, IDbTransaction? tran = null);
    Task SoftDeleteAsync(string id, IDbTransaction? tran = null);
    Task<List<Order>> GetLatestAsync(int count);
    Task<int> GetTodayCountAsync();
    Task<decimal> GetTotalRevenueAsync();
    Task<decimal> GetRevenueByDateRangeAsync(DateTime from, DateTime to);
    Task<int> GetInProgressCountByUserAsync(int userId);
    Task<List<Order>> GetInProgressByUserAsync(int userId);
}
