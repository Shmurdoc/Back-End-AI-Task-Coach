using FluentValidation;

namespace Application.Validators;

public class GoalDtoValidator : AbstractValidator<GoalDto>
{
    public GoalDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.UserId).NotEmpty();
    }
}
