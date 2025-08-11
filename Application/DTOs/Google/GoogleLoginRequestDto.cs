using System.ComponentModel.DataAnnotations;

namespace Application.DTO.Google
{
    public record GoogleLoginRequestDto(
    [Required] string IdToken
);
}
