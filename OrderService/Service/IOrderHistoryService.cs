using OrderService.Domain;

namespace OrderService.Service;
public interface IOrderHistoryService
{
    Task<bool> Create(OrderHistory model);
}