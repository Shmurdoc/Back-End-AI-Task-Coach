using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Login
{
    public record ResetPasswordRequestDto(
    [Required] string Email
);
}
