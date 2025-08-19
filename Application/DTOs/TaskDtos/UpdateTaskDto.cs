using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.TaskDtos
{
    public record UpdateTaskDto
    (
        string? Title = null,
        string? Description = null,
        DateTime? DueDate = null,
        int? Priority = null,
        string? Category = null,
        List<string>? Tags = null,
        TimeSpan? EstimatedDuration = null,
        bool? IsRecurring = null,
        string? RecurrencePattern = null
    );
}
