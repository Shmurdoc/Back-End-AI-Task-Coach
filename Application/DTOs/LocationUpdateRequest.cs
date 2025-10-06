using System.ComponentModel.DataAnnotations;

public record LocationUpdateRequest(
    [Required] Guid UserId,
    [Required] double Latitude,
    [Required] double Longitude,
    string? Description
);