using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using backend.Models;

namespace backend.Repositories;

public class UserRepository : IUserRepository
{
    private readonly string _connStr;
    public UserRepository(string connStr) => _connStr = connStr;

    private SqlConnection CreateConnection() => new(_connStr);

    public async Task<User?> GetByIdAsync(int id)
    {
        using var conn = CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE Id = @Id", new { Id = id });
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        using var conn = CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE Username = @Username", new { Username = username });
    }

    public async Task<PaginatedResult<User>> GetListAsync(string? search, string? status, int page, int pageSize)
    {
        using var conn = CreateConnection();
        var where = "WHERE Role != 'admin'";
        var parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(search))
        {
            where += " AND (Username LIKE @Search OR DisplayName LIKE @Search)";
            parameters.Add("Search", $"%{search}%");
        }
        if (!string.IsNullOrEmpty(status) && status != "all")
        {
            where += " AND Status = @Status";
            parameters.Add("Status", status);
        }

        var total = await conn.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM Users {where}", parameters);
        parameters.Add("Offset", (page - 1) * pageSize);
        parameters.Add("PageSize", pageSize);

        var items = await conn.QueryAsync<User>(
            $"SELECT * FROM Users {where} ORDER BY CreatedAt DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY",
            parameters);

        return new PaginatedResult<User> { Items = items.ToList(), Total = total, Page = page, PageSize = pageSize };
    }

    public async Task<int> CreateAsync(User user)
    {
        using var conn = CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            @"INSERT INTO Users (Username, PasswordHash, DisplayName, Phone, AvatarUrl, Role, Status)
              OUTPUT INSERTED.Id
              VALUES (@Username, @PasswordHash, @DisplayName, @Phone, @AvatarUrl, @Role, @Status)", user);
    }

    public async Task UpdateAsync(User user)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync(
            @"UPDATE Users SET DisplayName=@DisplayName, Phone=@Phone, AvatarUrl=@AvatarUrl, IsVip=@IsVip,
              Status=@Status, Balance=@Balance, UpdatedAt=GETUTCDATE() WHERE Id=@Id", user);
    }

    public async Task<decimal> GetBalanceAsync(int userId)
    {
        using var conn = CreateConnection();
        return await conn.ExecuteScalarAsync<decimal>(
            "SELECT Balance FROM Users WHERE Id = @Id", new { Id = userId });
    }

    public async Task RechargeAsync(int userId, decimal amount, IDbTransaction? tran = null)
    {
        const string sql = "UPDATE Users SET Balance = Balance + @Amount, UpdatedAt = GETUTCDATE() WHERE Id = @Id";
        if (tran != null)
            await tran!.Connection!.ExecuteAsync(sql, new { Id = userId, Amount = amount }, tran);
        else
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync(sql, new { Id = userId, Amount = amount });
        }
    }

    public async Task<bool> TryDeductBalanceAsync(int userId, decimal amount, IDbTransaction? tran = null)
    {
        const string sql = "UPDATE Users SET Balance = Balance - @Amount, UpdatedAt = GETUTCDATE() WHERE Id = @Id AND Balance >= @Amount AND Status = 'active'";
        int rows;
        if (tran != null)
            rows = await tran!.Connection!.ExecuteAsync(sql, new { Id = userId, Amount = amount }, tran);
        else
        {
            using var conn = CreateConnection();
            rows = await conn.ExecuteAsync(sql, new { Id = userId, Amount = amount });
        }
        return rows > 0;
    }

    public async Task<int> GetActiveCountAsync()
    {
        using var conn = CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Users WHERE Status = 'active'");
    }

    public async Task<User?> GetAdminAsync()
    {
        using var conn = CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE Role = 'admin'");
    }

    public async Task UpdateUsernameAsync(int id, string username)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE Users SET Username = @Username, UpdatedAt = GETUTCDATE() WHERE Id = @Id",
            new { Id = id, Username = username });
    }

    public async Task UpdatePasswordAsync(int id, string password)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE Users SET PasswordHash = @Password, UpdatedAt = GETUTCDATE() WHERE Id = @Id",
            new { Id = id, Password = password });
    }
}
