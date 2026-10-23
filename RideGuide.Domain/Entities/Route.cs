using RideGuide.Domain.Enums;

namespace RideGuide.Domain.Entities;

public class Route
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int RegionId { get; set; }
    public string StartPoint { get; set; } = string.Empty;
    public string EndPoint { get; set; } = string.Empty;
    public decimal? DistanceMiles { get; set; }
    public RoadCharacter RoadCharacter { get; set; }
    public Pace Pace { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
