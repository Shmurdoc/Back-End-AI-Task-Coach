using MediatR;

namespace Application.CQRS.Commands.Tasks;

public record DeleteTaskCommand(Guid TaskId) : IRequest<bool>;
