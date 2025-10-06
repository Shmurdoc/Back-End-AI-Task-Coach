using Application.DTOs.TaskDtos;
using Domain.Entities;
using MediatR;

namespace Application.CQRS.Features.Queries.Tasks;

public record GetUserTasksQuery(Guid UserId) : IRequest<IEnumerable<TaskDto>>;
