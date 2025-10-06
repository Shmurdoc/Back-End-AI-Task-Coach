namespace Application.DTOs;

public record ProductivitySummaryDto(
    int GoalsCompleted,
    int HabitsTracked,
    int TasksCompleted
);
