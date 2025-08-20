using MediatR;
using Domain.Entities;
using Domain.Enums;
using Application.DTOs.TaskDtos;

namespace Application.CQRS.Features.Commands.Tasks;

public record CreateTaskCommand(CreateTaskDto Dto) : IRequest<TaskDto>;
