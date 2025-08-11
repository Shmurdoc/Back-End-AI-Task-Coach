using System.ComponentModel.DataAnnotations;

namespace Application.DTO.Login
{
    public record ResetPasswordRequestDto(
    [Required] string Email
);
}
