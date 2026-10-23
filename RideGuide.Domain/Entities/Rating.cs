namespace RideGuide.Domain.Entities;

public class Rating
{
    public int Id { get; set; }
    public int RouteId { get; set; }
    public int UserId { get; set; }
    public int Score { get; set; }
    public string? Review { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
