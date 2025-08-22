using FluentValidation;

namespace Application.Validators;

public class HabitDtoValidator : AbstractValidator<HabitDto>
{
    public HabitDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Frequency).IsInEnum();
        RuleFor(x => x.UserId).NotEmpty();
    }
}
