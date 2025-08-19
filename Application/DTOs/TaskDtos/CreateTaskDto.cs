namespace Application.CQRS.Tasks;

public record CreateTaskDto(
    string Title,
    string Description,
    Guid? GoalId,
    int Priority,
    DateTime? DueDate
);
