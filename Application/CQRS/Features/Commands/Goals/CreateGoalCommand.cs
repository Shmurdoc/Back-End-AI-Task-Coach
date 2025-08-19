using Application.DTOs.GoalDtos;
using Domain.Entities;
using MediatR;

namespace Application.CQRS.Features.Commands.Goals;

public record CreateGoalCommand(CreateGoalDto CreateGoalDto) : IRequest<Goal>;
