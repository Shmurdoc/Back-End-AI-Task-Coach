using Application.CQRS.Commands.Goals;
using FluentValidation;

namespace Application.Validators;

public class CreateGoalCommandValidator : AbstractValidator<CreateGoalCommand>
{
    public CreateGoalCommandValidator()
    {

        RuleFor(x => x.Dto.Title)
            .NotEmpty()
            .WithMessage("Title is required")
            .MaximumLength(200)
            .WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Dto.Description)
            .MaximumLength(1000)
            .WithMessage("Description must not exceed 1000 characters");

        RuleFor(x => x.Dto.Priority)
            .InclusiveBetween(1, 5)
            .WithMessage("Priority must be between 1 and 5");

        RuleFor(x => x.Dto.TargetDate)
            .GreaterThan(DateTime.Today)
            .When(x => x.Dto.TargetDate.HasValue)
            .WithMessage("Target date must be in the future");
    }
}

