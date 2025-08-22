using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs.Analytics;

namespace Application.IService.Analytics
{
    public interface IEnergyMoodService
    {
        Task RecordEntryAsync(EnergyMoodEntryDto entry, CancellationToken ct = default);
        Task<IReadOnlyList<EnergyMoodEntryDto>> GetEntriesAsync(Guid userId, DateTime? from = null, DateTime? to = null, CancellationToken ct = default);
        Task<(double avgEnergy, double avgMood)> GetAveragesAsync(Guid userId, DateTime? from = null, DateTime? to = null, CancellationToken ct = default);
    }
}
