using Application.IService;

namespace Application.Services;



public class CalendarService : ICalendarService
{
    public async Task<bool> SyncWithGoogleCalendarAsync(Guid userId, string accessToken)
    {
        // TODO: Integrate with Google Calendar API
        return await Task.FromResult(true);
    }
}
