using Application.CQRS.Features.Queries.Goals;
using Application.DTOs.GoalDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IService
{
    public interface IGoalService
    {
        Task<IEnumerable<GoalDto>> GetUserGoal(Guid userId);
        Task<GoalDto> CreateGoalAsync(CreateGoalDto request, CancellationToken cancellationToken);
        Task CompleteGoalAsync(Guid goalId);
        Task<GoalDto?> GetGoalByIdAsync(Guid goalId, CancellationToken cancellationToken);
        Task<GoalDto> UpdateGoalAsync(Guid goalId, UpdateGoalDto dto, CancellationToken cancellationToken);
        Task<bool> DeleteGoalAsync(Guid goalId, CancellationToken cancellationToken);
        // ...other business logic signatures...
    }
}
