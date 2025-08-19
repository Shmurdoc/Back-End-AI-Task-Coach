using Application.DTOs.TaskDtos;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.CQRS.Features.Commands.Habits
{
    // UpdateGoalCommand.cs
    public record UpdateTaskCommand(Guid Id, UpdateTaskDto Dto) : IRequest<Task>;
}
