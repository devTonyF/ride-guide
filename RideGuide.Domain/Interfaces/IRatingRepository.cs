using RideGuide.Domain.Entities;

namespace RideGuide.Domain.Interfaces;

public interface IRatingRepository
{
    Task<IEnumerable<Rating>> GetByRouteIdAsync(int routeId);
    Task<Rating?> GetByIdAsync(int id);
    Task<Rating?> GetByRouteAndUserAsync(int routeId, int userId);
    Task<Rating> CreateAsync(Rating rating);
    Task<Rating> UpdateAsync(Rating rating);
    Task DeleteAsync(int id);
}
