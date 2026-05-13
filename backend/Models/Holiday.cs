namespace backend.Models;

public class Holiday
{
    public int Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal VipMultiplier { get; set; } = 1.5m;
    public decimal MediumMultiplier { get; set; } = 1.3m;
    public decimal SmallMultiplier { get; set; } = 1.2m;
    public DateTime CreatedAt { get; set; }
}
