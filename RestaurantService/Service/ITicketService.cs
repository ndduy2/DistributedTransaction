using RestaurantService.Domain;
using RestaurantService.Messing;

namespace RestaurantService.Service;
public interface ITicketService
{
    Task<Ticket> GetByOrderId(int orderId);
    Task Approve(CreateOrderMessage model);
    Task Done(CreateOrderMessage model);
    Task<int> UpdateStatus(int orderId, string status);
    Task<int> DeleteByOrderId(int orderId);
}