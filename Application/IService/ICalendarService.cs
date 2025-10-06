using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IService
{
    public interface ICalendarService
    {
        Task<bool> SyncWithGoogleCalendarAsync(Guid userId, string accessToken);
    }
}
