using Application.DTOs.Analytics;
using Application.IRepositories;
using Application.IService.Analytics;
using Domain.Entities;

namespace Application.Services.Analytics;

/// <summary>
/// Persists energy/mood check-ins through the repository layer. The old
/// process-local demo dictionary is gone; entries live in the database.
/// Adds are staged by the repository and committed by the unit of work
/// (repository convention).
/// </summary>
public class EnergyMoodService : IEnergyMoodService
{
    private readonly IEnergyMoodRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public EnergyMoodService(IEnergyMoodRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task RecordEntryAsync(EnergyMoodEntryDto entry, CancellationToken ct = default)
    {
        await _repository.AddAsync(new EnergyMoodEntry
        {
            Id = Guid.NewGuid(),
            UserId = entry.UserId,
            Timestamp = entry.Timestamp,
            EnergyLevel = entry.EnergyLevel,
            MoodLevel = entry.MoodLevel,
            Note = entry.Note
        });
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<EnergyMoodEntryDto>> GetEntriesAsync(Guid userId, DateTime? from = null, DateTime? to = null, CancellationToken ct = default)
    {
        var entries = await _repository.GetByUserAsync(userId, from, to);
        return entries
            .Select(e => new EnergyMoodEntryDto(e.UserId, e.Timestamp, e.EnergyLevel, e.MoodLevel, e.Note))
            .ToList();
    }

    public async Task<(double avgEnergy, double avgMood)> GetAveragesAsync(Guid userId, DateTime? from = null, DateTime? to = null, CancellationToken ct = default)
    {
        var entries = await GetEntriesAsync(userId, from, to, ct);
        if (entries.Count == 0) return (0, 0);
        return (entries.Average(e => e.EnergyLevel), entries.Average(e => e.MoodLevel));
    }
}
