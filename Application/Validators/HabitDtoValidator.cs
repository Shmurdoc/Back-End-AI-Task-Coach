
using FluentValidation;
using Application.DTOs.HabitDtos;

namespace Application.Validators;

public class HabitDtoValidator : AbstractValidator<HabitDto>
{
    public HabitDtoValidator()
    {
    RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Frequency).IsInEnum();
        RuleFor(x => x.UserId).NotEmpty();
    }
}
