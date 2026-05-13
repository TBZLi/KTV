using Dapper;
using Microsoft.Data.SqlClient;
using backend.Models;

namespace backend.Repositories;

public class HolidayRepository : IHolidayRepository
{
    private readonly string _connStr;
    public HolidayRepository(string connStr) => _connStr = connStr;

    private SqlConnection CreateConnection() => new(_connStr);

    public async Task<List<Holiday>> GetAllAsync()
    {
        using var conn = CreateConnection();
        var items = await conn.QueryAsync<Holiday>(
            "SELECT * FROM Holidays ORDER BY StartDate DESC");
        return items.ToList();
    }

    public async Task<Holiday?> GetByIdAsync(int id)
    {
        using var conn = CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<Holiday>(
            "SELECT * FROM Holidays WHERE Id = @Id", new { Id = id });
    }

    public async Task<int> CreateAsync(Holiday holiday)
    {
        using var conn = CreateConnection();
        var id = await conn.ExecuteScalarAsync<int>(
            @"INSERT INTO Holidays (StartDate, EndDate, VipMultiplier, MediumMultiplier, SmallMultiplier)
              VALUES (@StartDate, @EndDate, @VipMultiplier, @MediumMultiplier, @SmallMultiplier);
              SELECT CAST(SCOPE_IDENTITY() AS INT);",
            holiday);
        return id;
    }

    public async Task DeleteAsync(int id)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync("DELETE FROM Holidays WHERE Id = @Id", new { Id = id });
    }

    public async Task<List<Holiday>> GetActiveHolidaysAsync(DateTime date)
    {
        using var conn = CreateConnection();
        var dateOnly = date.Date;
        var items = await conn.QueryAsync<Holiday>(
            "SELECT * FROM Holidays WHERE @Date >= StartDate AND @Date <= EndDate",
            new { Date = dateOnly });
        return items.ToList();
    }
}
