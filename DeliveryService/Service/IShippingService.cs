using DeliveryService.Domain;

namespace DeliveryService.Service;
public interface IShippingService
{
    Task<string> FindShipper(int cf);
    Task<Shipping> GetByOrderId(int orderId);
    Task<bool> CreateShipping(Shipping shipping);
    Task<bool> UpdateRestaurantStatus(int orderId, string restaurantStatus);
    Task<bool> UpdateShipper(int orderId, string shipper);
}