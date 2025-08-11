using System.ComponentModel.DataAnnotations;

namespace Application.DTO;

public record WeeklyReportRequest(
    [Required] Guid UserId,
    [Required, EmailAddress] string Email
);
