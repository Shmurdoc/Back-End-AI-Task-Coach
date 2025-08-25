
using FluentValidation;
using Application.DTOs.AuthDtos;

namespace Application.Validators;

public class UserDtoValidator : AbstractValidator<UserDto>
{
    public UserDtoValidator()
    {
    RuleFor(x => x.Email).NotEmpty().EmailAddress();
    RuleFor(x => x.Name).NotEmpty();
    }
}
