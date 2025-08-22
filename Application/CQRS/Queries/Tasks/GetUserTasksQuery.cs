using Application.DTOs.TaskDtos;
using MediatR;

namespace Application.CQRS.Queries.Tasks;

public record GetUserTasksQuery(Guid UserId) : IRequest<IEnumerable<TaskDto>>;
