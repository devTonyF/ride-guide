using RideGuide.Domain.Enums;

namespace RideGuide.Domain.Models;

public class RouteQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int? RegionId { get; set; }
    public Pace? Pace { get; set; }
    public RoadCharacter? RoadCharacter { get; set; }
}
