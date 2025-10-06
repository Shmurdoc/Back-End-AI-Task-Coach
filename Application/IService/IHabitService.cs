using Application.DTOs.HabitDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        // For controller compatibility:
        Task<HabitDto?> GetHabitAsync(Guid id);
        Task<HabitDto?> CreateHabitAsync(HabitDto dto);
        Task UpdateHabitAsync(HabitDto dto);
        Task DeleteHabitAsync(Guid id);
    }
}
