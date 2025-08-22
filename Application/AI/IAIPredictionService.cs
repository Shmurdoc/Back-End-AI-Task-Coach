using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs.TaskDtos;
using Application.DTOs.GoalDtos;

namespace Application.AI
{
    /// <summary>
    /// Interface for AI/ML-based prediction and suggestion services.
    /// </summary>
    public interface IAIPredictionService
    {
        /// <summary>
        /// Predicts the likelihood of task completion and suggests optimal scheduling.
        /// </summary>
        Task<TaskPredictionResult> PredictTaskAsync(TaskDto task, Guid userId);

        /// <summary>
        /// Suggests next best actions or nudges for a user based on their history.
        /// </summary>
        Task<IEnumerable<string>> GetSuggestionsAsync(Guid userId);

        /// <summary>
        /// Predicts at-risk goals and recommends decompositions or timeline adjustments.
        /// </summary>
        Task<GoalPredictionResult> PredictGoalAsync(GoalDto goal, Guid userId);
    }

    public record TaskPredictionResult(bool IsAtRisk, DateTime? SuggestedDueDate, string? Nudge);
    public record GoalPredictionResult(bool IsAtRisk, IEnumerable<string> DecompositionSuggestions);
}
