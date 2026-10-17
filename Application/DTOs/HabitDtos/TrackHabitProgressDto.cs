namespace Application.DTOs.HabitDtos;

/// <summary>
/// Progress log for a habit (e.g. "8 of 8 glasses of water").
/// </summary>
public record TrackHabitProgressDto(int Value);
