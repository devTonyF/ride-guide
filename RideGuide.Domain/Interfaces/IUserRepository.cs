using RideGuide.Domain.Entities;

namespace RideGuide.Domain.Interfaces;

public interface IUserRepository
{
    Task<AppUser?> GetByIdAsync(int id);
    Task<AppUser> UpdateAsync(AppUser user);
    Task DeleteAsync(int id);
}
