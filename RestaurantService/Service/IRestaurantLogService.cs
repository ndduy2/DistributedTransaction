using RestaurantService.Domain;

namespace RestaurantService.Service;
public interface IRestaurantLogService
{
    Task<bool> Create(RestaurantLog model);
    Task<bool> UpdateStatus(int orderId, bool isCooking);
    Task<RestaurantLog> GetByOrderId(int orderId);
}