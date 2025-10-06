using Application.DTOs.GoalDtos;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.CQRS.Features.Commands.Goals
{
    // DeleteGoalCommand.cs
    public record DeleteGoalCommand(Guid Id) : IRequest<bool>;
}
