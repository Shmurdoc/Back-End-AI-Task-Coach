using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.HabitDtos
{
    public record CreateHabitDto(
        string Name,
        string Description, 
        DateTime StartDate, 
        DateTime EndDate, 
        int Frequency, 
        bool IsActive);

}
