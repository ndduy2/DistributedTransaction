using Common;
using RestaurantService.Domain;
using RestaurantService.Messing;

namespace RestaurantService.Service;
public interface IInventoryService
{
    Task<Inventory> GetAmount(string product);
    Task UpdateStock(CreateOrderMessage model, int stock, string status = CoreConstant.InventoryStatus.LOCKING);
    Task RevertStock(string product, int stock);
}