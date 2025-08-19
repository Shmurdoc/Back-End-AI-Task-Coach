using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Google
{
    public record GoogleLoginRequestDto(
    [Required] string IdToken
);
}
