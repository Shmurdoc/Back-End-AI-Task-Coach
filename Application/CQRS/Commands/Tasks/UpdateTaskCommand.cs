using Application.DTOs.TaskDtos;
using MediatR;

namespace Application.CQRS.Commands.Tasks;

public record UpdateTaskCommand(Guid TaskId, UpdateTaskDto Dto) : IRequest<TaskDto>;
