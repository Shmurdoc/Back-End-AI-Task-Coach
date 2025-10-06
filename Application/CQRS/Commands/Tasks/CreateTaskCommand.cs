using Application.DTOs.TaskDtos;
using MediatR;

namespace Application.CQRS.Commands.Tasks;

public record CreateTaskCommand(CreateTaskDto Dto) : IRequest<TaskDto>;
