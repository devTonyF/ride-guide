using RideGuide.Domain.Entities;

namespace RideGuide.Domain.Interfaces;

public interface IRegionRepository
{
    Task<IEnumerable<Region>> GetAllAsync();
    Task<Region?> GetByIdAsync(int id);
    Task<Region> CreateAsync(Region region);
}
