using System;

namespace Application.DTOs.Analytics
{
    public record EnergyMoodEntryDto(Guid UserId, DateTime Timestamp, int EnergyLevel, int MoodLevel, string? Note = null);
}
