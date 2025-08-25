
using FluentValidation;
using Application.DTOs.GoalDtos;

namespace Application.Validators;

public class GoalDtoValidator : AbstractValidator<GoalDto>
{
    public GoalDtoValidator()
    {
    RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
    }
}
