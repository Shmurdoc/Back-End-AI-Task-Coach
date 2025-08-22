using Application.DTOs.TaskDtos;
using FluentValidation;

namespace Application.Validators;

public class CreateTaskDtoValidator : AbstractValidator<CreateTaskDto>
{
    public CreateTaskDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EstimatedHours).GreaterThan(0);
        RuleFor(x => x.Priority).IsInEnum();
    }
}
