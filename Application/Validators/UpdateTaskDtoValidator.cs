using Application.DTOs.TaskDtos;
using FluentValidation;

namespace Application.Validators;

public class UpdateTaskDtoValidator : AbstractValidator<UpdateTaskDto>
{
    public UpdateTaskDtoValidator()
    {
        When(x => x.Title != null, () => RuleFor(x => x.Title).NotEmpty().MaximumLength(200));
        When(x => x.EstimatedHours.HasValue, () => RuleFor(x => x.EstimatedHours.Value).GreaterThan(0));
    }
}
