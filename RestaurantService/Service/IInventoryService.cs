using RestaurantService.Domain;

namespace RestaurantService.Service;
public interface IInventoryService
{
    Task<Inventory> GetAmount(string product);
    Task UpdateStock(string product, int stock, int? orderId = null);
}