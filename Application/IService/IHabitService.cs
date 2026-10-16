using Application.DTOs.HabitDtos;

namespace Application.IService
{
    public interface IHabitService
    {
        Task<IEnumerable<HabitDto>> GetUserHabitsAsync(Guid userId);
        Task<HabitDto?> CreateHabitAsync(CreateHabitDto dto, CancellationToken cancellationToken);
        Task TrackHabitProgressAsync(Guid habitId, int value);
        Task<HabitDto?> GetHabitByIdAsync(Guid habitId, CancellationToken cancellationToken);
        Task<HabitDto> UpdateHabitAsync(Guid habitId, UpdateHabitDto dto, CancellationToken cancellationToken);
        Task<bool> DeleteHabitAsync(Guid habitId, CancellationToken cancellationToken);
    }
}
