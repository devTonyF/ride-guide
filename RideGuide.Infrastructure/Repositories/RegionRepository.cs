using Microsoft.EntityFrameworkCore;
using RideGuide.Domain.Entities;
using RideGuide.Domain.Interfaces;
using RideGuide.Infrastructure.Data;

namespace RideGuide.Infrastructure.Repositories;

public class RegionRepository : IRegionRepository
{
    private readonly AppDbContext _context;

    public RegionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Region>> GetAllAsync()
    {
        return await _context.Regions.ToListAsync();
    }

    public async Task<Region?> GetByIdAsync(int id)
    {
        return await _context.Regions.FindAsync(id);
    }

    public async Task<Region> CreateAsync(Region region)
    {
        _context.Regions.Add(region);
        await _context.SaveChangesAsync();
        return region;
    }
}
