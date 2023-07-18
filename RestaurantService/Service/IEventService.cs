using RestaurantService.Domain;

namespace RestaurantService.Service;
public interface IEventService
{
    Task<int> CreateEvent(Event ev);
    Task<Event> GetNextPendingEvent();
    Task<bool> UpdateEventStatus(int id, string status);
}