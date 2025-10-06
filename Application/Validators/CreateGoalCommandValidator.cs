using Application.CQRS.Features.Commands.Goals;
using FluentValidation;

namespace Application.Validators;

public class CreateGoalCommandValidator : AbstractValidator<CreateGoalCommand>
{
    public CreateGoalCommandValidator()
    {

        RuleFor(x => x.CreateGoalDto.Title)
            .NotEmpty()
            .WithMessage("Title is required")
            .MaximumLength(200)
            .WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.CreateGoalDto.Description)
            .MaximumLength(1000)
            .WithMessage("Description must not exceed 1000 characters");

        RuleFor(x => x.CreateGoalDto.Priority)
            .InclusiveBetween(1, 5)
            .WithMessage("Priority must be between 1 and 5");

        RuleFor(x => x.CreateGoalDto.TargetDate)
            .GreaterThan(DateTime.Today)
            .When(x => x.CreateGoalDto.TargetDate.HasValue)
            .WithMessage("Target date must be in the future");
    }
}

