using FluentValidation;

namespace Application.Validators;

public class TaskDtoValidator : AbstractValidator<TaskDto>
{
    public TaskDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EstimatedHours).GreaterThanOrEqualTo(0.1);
        RuleFor(x => x.UserId).NotEmpty();
    }
}
