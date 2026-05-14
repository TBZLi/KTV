using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using backend.Models;

namespace backend.Repositories;

public class RoomRepository : IRoomRepository
{
    private readonly string _connStr;
    public RoomRepository(string connStr) => _connStr = connStr;

    private SqlConnection CreateConnection() => new(_connStr);

    public async Task<Room?> GetByIdAsync(int id)
    {
        using var conn = CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<Room>(
            "SELECT * FROM Rooms WHERE Id = @Id", new { Id = id });
    }

    public async Task<PaginatedResult<Room>> GetListAsync(string? search, string? status, int page, int pageSize)
    {
        using var conn = CreateConnection();
        var where = "WHERE 1=1";
        var parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(search))
        {
            where += " AND RoomNumber LIKE @Search";
            parameters.Add("Search", $"%{search}%");
        }
        if (!string.IsNullOrEmpty(status) && status != "all")
        {
            where += " AND Status = @Status";
            parameters.Add("Status", status);
        }

        var total = await conn.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM Rooms {where}", parameters);
        parameters.Add("Offset", (page - 1) * pageSize);
        parameters.Add("PageSize", pageSize);

        var items = await conn.QueryAsync<Room>(
            $"SELECT * FROM Rooms {where} ORDER BY RoomNumber ASC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY",
            parameters);

        return new PaginatedResult<Room> { Items = items.ToList(), Total = total, Page = page, PageSize = pageSize };
    }

    public async Task UpdateStatusAsync(int id, string status, IDbTransaction? tran = null)
    {
        const string sql = "UPDATE Rooms SET Status = @Status, UpdatedAt = GETUTCDATE() WHERE Id = @Id";
        if (tran != null)
            await tran!.Connection!.ExecuteAsync(sql, new { Id = id, Status = status }, tran);
        else
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync(sql, new { Id = id, Status = status });
        }
    }

    public async Task UpdateCurrentOrderIdAsync(int id, string? orderId, IDbTransaction? tran = null)
    {
        const string sql = "UPDATE Rooms SET CurrentOrderId = @OrderId, UpdatedAt = GETUTCDATE() WHERE Id = @Id";
        if (tran != null)
            await tran!.Connection!.ExecuteAsync(sql, new { Id = id, OrderId = orderId }, tran);
        else
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync(sql, new { Id = id, OrderId = orderId });
        }
    }

    public async Task<int> GetCountAsync()
    {
        using var conn = CreateConnection();
        return await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Rooms");
    }
}
