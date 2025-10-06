using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public record WeeklyReportRequest(
    [Required] Guid UserId,
    [Required, EmailAddress] string Email
);
