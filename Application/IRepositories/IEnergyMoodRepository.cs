using Domain.Entities;

namespace Application.IRepositories;

public interface IEnergyMoodRepository
{
    Task<EnergyMoodEntry> AddAsync(EnergyMoodEntry entry);
    Task<IEnumerable<EnergyMoodEntry>> GetByUserAsync(Guid userId, DateTime? from = null, DateTime? to = null);
}
