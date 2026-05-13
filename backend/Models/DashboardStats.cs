namespace backend.Models;

public class DashboardStats
{
    public int TotalRooms { get; set; }
    public int TodayOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int ActiveUsers { get; set; }
}

public class SongStats
{
    public int TotalSongs { get; set; }
    public int WeeklyNew { get; set; }
    public int TodayPlays { get; set; }
}

public class PaginatedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class SystemSettingsDto
{
    public string StoreName { get; set; } = string.Empty;
    public string StorePhone { get; set; } = string.Empty;
    public string StoreAddress { get; set; } = string.Empty;
    public string BusinessHours { get; set; } = string.Empty;
    public bool HolidayPricingEnabled { get; set; }
    public decimal BaseHourlyRate { get; set; }
    public int LogRetentionDays { get; set; }
    public bool SensitiveOpVerification { get; set; }
}
