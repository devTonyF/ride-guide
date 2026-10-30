using RideGuide.Domain.Entities;
using RideGuide.Domain.Models;

namespace RideGuide.Domain.Interfaces;

public interface IRouteRepository
{
    Task<PagedResult<Route>> GetAllAsync(RouteQuery query);
    Task<Route?> GetByIdAsync(int id);
    Task<Route> CreateAsync(Route route);
    Task<Route> UpdateAsync(Route route);
    Task DeleteAsync(int id);
}
