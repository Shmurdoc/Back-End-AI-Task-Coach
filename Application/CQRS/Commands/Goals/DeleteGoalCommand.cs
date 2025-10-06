using MediatR;

namespace Application.CQRS.Commands.Goals;

public record DeleteGoalCommand(Guid GoalId) : IRequest<bool>;
