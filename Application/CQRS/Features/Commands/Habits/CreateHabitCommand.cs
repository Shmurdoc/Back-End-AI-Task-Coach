using MediatR;
using Domain.Entities;
using Domain.Enums;
using Application.DTOs.HabitDtos;

namespace Application.CQRS.Features.Commands.Habits;

public record CreateHabitCommand(CreateHabitDto CreateHabitDto) : IRequest<HabitDto>;
