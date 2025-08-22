using Application.DTOs.TaskDtos;
using MediatR;

namespace Application.CQRS.Queries.Tasks;

public record GetTaskByIdQuery(Guid TaskId) : IRequest<TaskDto?>;
