using OrderService.Domain;

namespace OrderService.Service;
public interface IOrderService
{
    Task<Order> GetOneById(int orderId);
    Task<int> CreateOrder(Order order);
    Task<int> UpdateOrder(int orderId, string status);
    Task<int> UpdateOrderShipper(int orderId, string shipper);
    Task<int> DeleteOrder(int orderId);
}