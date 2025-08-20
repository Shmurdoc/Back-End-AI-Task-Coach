using Application.DTOs.HabitDtos;
using Domain.Entities;
using MediatR;

namespace Application.CQRS.Features.Queries.Habits;

public record GetHabitsByIdQuery(Guid HabitId, Guid UserId) : IRequest<HabitDto?>;
