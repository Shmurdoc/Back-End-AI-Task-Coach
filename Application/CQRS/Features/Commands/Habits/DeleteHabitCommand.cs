using Application.DTOs.GoalDtos;
using Application.DTOs.HabitDtos;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.CQRS.Features.Commands.Habits
{
    // DeleteGoalCommand.cs
    public record DeleteHabitCommand(Guid Id) : IRequest;
}
