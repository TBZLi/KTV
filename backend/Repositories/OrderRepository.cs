using Dapper;
using Microsoft.Data.SqlClient;
using backend.Models;

namespace backend.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly string _connStr;
    public OrderRepository(string connStr) => _connStr = connStr;

    private SqlConnection CreateConnection() => new(_connStr);

    private const string JoinSql = @"FROM Orders o
              LEFT JOIN Rooms r ON o.RoomId = r.Id
              LEFT JOIN Users u ON o.UserId = u.Id";

    public async Task<Order?> GetByIdAsync(string id)
    {
        using var conn = CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<Order>(
            $@"SELECT o.*, r.RoomNumber, r.RoomType, u.Username
              {JoinSql}
              WHERE o.Id = @Id", new { Id = id });
    }

    public async Task<PaginatedResult<Order>> GetListAsync(string? status, string? searchField, string? searchKeyword, int page, int pageSize)
    {
        using var conn = CreateConnection();
        var where = "WHERE o.IsDeleted = 0";
        var parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(status) && status != "all")
        {
            where += " AND o.Status = @Status";
            parameters.Add("Status", status);
        }

        if (!string.IsNullOrEmpty(searchKeyword) && !string.IsNullOrEmpty(searchField))
        {
            switch (searchField)
            {
                case "orderId":
                    where += " AND o.Id LIKE @SearchKeyword";
                    break;
                case "username":
                    where += " AND u.Username LIKE @SearchKeyword";
                    break;
                case "roomNumber":
                    where += " AND r.RoomNumber LIKE @SearchKeyword";
                    break;
            }
            parameters.Add("SearchKeyword", $"%{searchKeyword}%");
        }

        var useJoin = !string.IsNullOrEmpty(searchKeyword);
        var totalSql = useJoin
            ? $"SELECT COUNT(*) {JoinSql} {where}"
            : $"SELECT COUNT(*) FROM Orders o {where}";
        var total = await conn.ExecuteScalarAsync<int>(totalSql, parameters);
        parameters.Add("Offset", (page - 1) * pageSize);
        parameters.Add("PageSize", pageSize);

        var items = await conn.QueryAsync<Order>(
            $@"SELECT o.*, r.RoomNumber, r.RoomType, u.Username
               {JoinSql}
               {where}
               ORDER BY o.CreatedAt DESC
               OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY",
            parameters);

        return new PaginatedResult<Order> { Items = items.ToList(), Total = total, Page = page, PageSize = pageSize };
    }

    public async Task<string> CreateAsync(Order order)
    {
        using var conn = CreateConnection();
        order.Id = $"ORD{DateTime.UtcNow:yyMMddHHmm}{new Random().Next(100, 999)}";
        await conn.ExecuteAsync(
            @"INSERT INTO Orders (Id, UserId, RoomId, OrderType, SongId, Amount, Status, StartTime, EndTime)
              VALUES (@Id, @UserId, @RoomId, @OrderType, @SongId, @Amount, @Status, @StartTime, @EndTime)",
            order);
        return order.Id;
    }

    public async Task RefundAsync(string id)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE Orders SET Status = 'refunded', EndTime = GETUTCDATE(), UpdatedAt = GETUTCDATE() WHERE Id = @Id",
            new { Id = id });
    }

    public async Task CompleteAsync(string id)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE Orders SET Status = 'completed', EndTime = GETUTCDATE(), UpdatedAt = GETUTCDATE() WHERE Id = @Id",
            new { Id = id });
    }

    public async Task CancelAsync(string id)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE Orders SET Status = 'cancelled', EndTime = GETUTCDATE(), UpdatedAt = GETUTCDATE() WHERE Id = @Id",
            new { Id = id });
    }

    public async Task RestoreAsync(string id)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE Orders SET Status = 'in_progress', EndTime = NULL, UpdatedAt = GETUTCDATE() WHERE Id = @Id",
            new { Id = id });
    }

    public async Task SoftDeleteAsync(string id)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE Orders SET IsDeleted = 1, UpdatedAt = GETUTCDATE() WHERE Id = @Id",
            new { Id = id });
    }

    public async Task<List<Order>> GetLatestAsync(int count)
    {
        using var conn = CreateConnection();
        var items = await conn.QueryAsync<Order>(
            $@"SELECT TOP(@Count) o.*, r.RoomNumber, r.RoomType, u.Username
               {JoinSql}
               WHERE o.IsDeleted = 0
               ORDER BY o.CreatedAt DESC",
            new { Count = count });
        return items.ToList();
    }

    public async Task<int> GetTodayCountAsync()
    {
        using var conn = CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Orders WHERE IsDeleted = 0 AND CAST(CreatedAt AS DATE) = CAST(GETUTCDATE() AS DATE)");
    }

    public async Task<decimal> GetTotalRevenueAsync()
    {
        using var conn = CreateConnection();
        return await conn.ExecuteScalarAsync<decimal>(
            "SELECT ISNULL(SUM(Amount), 0) FROM Orders WHERE IsDeleted = 0 AND Status IN ('in_progress', 'completed')");
    }

    public async Task<decimal> GetRevenueByDateRangeAsync(DateTime from, DateTime to)
    {
        using var conn = CreateConnection();
        return await conn.ExecuteScalarAsync<decimal>(
            @"SELECT ISNULL(SUM(Amount), 0) FROM Orders
              WHERE IsDeleted = 0 AND Status IN ('in_progress', 'completed')
              AND CreatedAt >= @From AND CreatedAt < @To",
            new { From = from, To = to });
    }

    public async Task<int> GetInProgressCountByUserAsync(int userId)
    {
        using var conn = CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Orders WHERE UserId = @UserId AND Status = 'in_progress' AND IsDeleted = 0",
            new { UserId = userId });
    }

    public async Task<List<Order>> GetInProgressByUserAsync(int userId)
    {
        using var conn = CreateConnection();
        var items = await conn.QueryAsync<Order>(
            "SELECT * FROM Orders WHERE UserId = @UserId AND Status = 'in_progress' AND IsDeleted = 0",
            new { UserId = userId });
        return items.ToList();
    }
}
