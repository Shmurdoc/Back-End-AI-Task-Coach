using Application.CQRS.Features.Queries.Goals;
using Application.DTOs.GoalDtos;
using Application.IRepositories;
using Application.Queries.Goals.Application.Mappers;
using Domain.Entities;
using MediatR;

namespace Application.Queries.Goals
{
    public class GetGoalByIdQueryHandler : IRequestHandler<GetGoalByIdQuery, Goal?>
    {
        private readonly IGoalRepository _goalRepository;

        public GetGoalByIdQueryHandler(IGoalRepository goalRepository)
        {
            _goalRepository = goalRepository;
        }



        public async Task<GoalDto?> Handle(GetGoalByIdQuery request, CancellationToken cancellationToken)
        {
            // 1. Retrieve the goal by its ID from the repository.
            var goal = await _goalRepository.GetByIdAsync(request.GoalId);

            // 2. Check if the goal exists and if it belongs to the requesting user.
            // This is a critical security check to prevent a user from accessing another user's goals.
            if (goal == null || goal.UserId != request.UserId)
            {
                return null; // Return null if the goal is not found or unauthorized.
            }

            // 3. Map the Goal entity to a GoalDto for the API response.
            // This abstraction prevents internal domain models from being exposed directly.
            return goal.ToDto();
        }
    }


    namespace Application.Mappers
    {
        // A simple mapper to convert the domain entity to its DTO representation.
        public static class GoalMapper
        {
            public static GoalDto ToDto(this Goal goal)
            {
                // You would have a method to calculate the progress here.
                // For now, it's set to 0.
                double progress = 0;
                return new GoalDto(
                    goal.Id,
                    goal.Title,
                    goal.Description,
                    goal.Category,
                    goal.Status,
                    goal.Priority,
                    goal.TargetDate,
                    goal.CreatedAt,
                    goal.CompletedAt,
                    progress
                );
            }




        }
    }
}