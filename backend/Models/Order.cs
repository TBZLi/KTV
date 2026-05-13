namespace backend.Models;

public class Order
{
    public string Id { get; set; } = string.Empty;
    public int UserId { get; set; }
    public int RoomId { get; set; }
    public string OrderType { get; set; } = "room";
    public int? SongId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "in_progress";
    public bool IsDeleted { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    // Joined fields
    public string? RoomNumber { get; set; }
    public string? RoomType { get; set; }
    public string? Username { get; set; }
}
