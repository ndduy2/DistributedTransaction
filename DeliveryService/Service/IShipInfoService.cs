using DeliveryService.Domain;
using DeliveryService.Messing;

namespace DeliveryService.Service;
public interface IShipInfoService
{
    Task FindShipper(CreateOrderMessage model);
    // Task<ShipInfo> GetByOrderId(int orderId);
    // Task<bool> CreateShipping(ShipInfo shipping);
    // Task<bool> UpdateRestaurantStatus(int orderId, string restaurantStatus);
    // Task<bool> UpdateShipper(int orderId, string shipper);
}