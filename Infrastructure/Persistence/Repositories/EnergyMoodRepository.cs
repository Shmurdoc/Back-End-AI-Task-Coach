using Application.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Energy/mood check-in rows. Adds are staged only; the unit of work
/// commits (repository convention).
/// </summary>
public class EnergyMoodRepository : IEnergyMoodRepository
{
    private readonly ApplicationDbContext _context;

    public EnergyMoodRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EnergyMoodEntry> AddAsync(EnergyMoodEntry entry)
    {
        await _context.EnergyMoodEntries.AddAsync(entry);
        return entry;
    }

    public async Task<IEnumerable<EnergyMoodEntry>> GetByUserAsync(Guid userId, DateTime? from = null, DateTime? to = null)
    {
        var query = _context.EnergyMoodEntries
            .AsNoTracking()
            .Where(e => e.UserId == userId);

        if (from.HasValue)
            query = query.Where(e => e.Timestamp >= from.Value);
        if (to.HasValue)
            query = query.Where(e => e.Timestamp <= to.Value);

        return await query.OrderBy(e => e.Timestamp).ToListAsync();
    }
}
