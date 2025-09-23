using System;
using System.Collections.Generic;

namespace Application.DTOs.GoalDtos
{
    public record GoalDecompositionDto(
        Guid GoalId,
        string GoalTitle,
        List<string> Subtasks,
        string? AIExplanation = null
    );
}
