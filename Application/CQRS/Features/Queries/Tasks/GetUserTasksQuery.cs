using MediatR;
using Domain.Entities;

namespace Application.CQRS.Features.Queries.Tasks;

public record GetUserTasksQuery(Guid UserId) : IRequest<IEnumerable<TaskItem>>;
