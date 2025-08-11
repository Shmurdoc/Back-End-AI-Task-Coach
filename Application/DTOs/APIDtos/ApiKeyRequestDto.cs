using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.APIDtos;


public class ApiKeyRequest
{
    [Required, StringLength(100)]
    public string ServiceName { get; set; } = string.Empty;

    [Required]
    public List<string> Permissions { get; set; } = new();

    public string? Description { get; set; }
}
