using Application.DTOs.Analytics;
using Application.IService.Analytics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.Analytics
{
    public class EnergyMoodService : IEnergyMoodService
    {
        // In-memory store for demo; replace with persistent storage in production
        private static readonly ConcurrentDictionary<Guid, List<EnergyMoodEntryDto>> _entries = new();

        public async Task RecordEntryAsync(EnergyMoodEntryDto entry, CancellationToken ct = default)
        {
            var list = _entries.GetOrAdd(entry.UserId, _ => new List<EnergyMoodEntryDto>());
            lock (list) { list.Add(entry); }
            await Task.CompletedTask;
        }

        public async Task<IReadOnlyList<EnergyMoodEntryDto>> GetEntriesAsync(Guid userId, DateTime? from = null, DateTime? to = null, CancellationToken ct = default)
        {
            if (!_entries.TryGetValue(userId, out var list)) return Array.Empty<EnergyMoodEntryDto>();
            var result = list.Where(e => (!from.HasValue || e.Timestamp >= from) && (!to.HasValue || e.Timestamp <= to)).ToList();
            return await Task.FromResult(result);
        }

        public async Task<(double avgEnergy, double avgMood)> GetAveragesAsync(Guid userId, DateTime? from = null, DateTime? to = null, CancellationToken ct = default)
        {
            var entries = await GetEntriesAsync(userId, from, to, ct);
            if (entries.Count == 0) return (0, 0);
            return (entries.Average(e => e.EnergyLevel), entries.Average(e => e.MoodLevel));
        }
    }
}
