using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;
using DeliveryService.Domain;
using DeliveryService.Messing;
using Common;
using Newtonsoft.Json;

namespace DeliveryService.Service;
public class ShipInfoService : IShipInfoService
{
    private readonly NpgsqlDataSource _ds;

    public ShipInfoService(IConfiguration configuration)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(configuration.GetConnectionString("DB"));
        _ds = dataSourceBuilder.Build();
    }

    public async Task FindShipper(CreateOrderMessage model)
    {
        try
        {
            // so luong le => khong tim dk shipper
            if (model.Quantity % 2 == 0)
            {
                model.Shipper = "SHIPPER A";
                using var connection = _ds.OpenConnection();
                {
                    var cmd = new NpgsqlCommand();
                    cmd.Connection = connection;
                    cmd.CommandText = $"INSERT INTO public.event (type, data, status) VALUES (@type, @data, @status)";
                    cmd.Parameters.AddWithValue("type", CoreConstant.Topic.SHIPPER_FOUND);
                    cmd.Parameters.AddWithValue("data", JsonConvert.SerializeObject(model));
                    cmd.Parameters.AddWithValue("status", CoreConstant.EventStatus.NEW);
                    cmd.Prepare();
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            else
            {
                throw new Exception("Failed");
            }
        }
        catch (System.Exception ex)
        {
            throw new Exception("Failed");
        }
    }

    // public async Task<ShipInfo> GetByOrderId(int orderId)
    // {
    //     try
    //     {
    //         List<ShipInfo> result = new List<ShipInfo>();
    //         using var connection = _ds.OpenConnection();
    //         {
    //             var cmd = new NpgsqlCommand();
    //             cmd.Connection = connection;
    //             cmd.CommandText = $"SELECT * FROM public.shipping WHERE order_id = @order_id";
    //             cmd.Parameters.AddWithValue("order_id", orderId);
    //             cmd.Prepare();
    //             NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
    //             while (await reader.ReadAsync())
    //             {
    //                 result.Add(new ShipInfo()
    //                 {
    //                     OrderId = (int)reader["order_id"],
    //                     Shipper = reader["shipper"] as string,
    //                     GoodsStatus = reader["goods_status"] as string,
    //                     Status = reader["status"] as string,
    //                 });
    //             }

    //         }
    //         return result.FirstOrDefault();
    //     }
    //     catch (System.Exception ex)
    //     {
    //         Console.WriteLine($"Ex: {ex.Message} \n {ex.StackTrace}");
    //         throw ex;
    //     }
    // }

    // public async Task<bool> CreateShipping(ShipInfo shipping)
    // {
    //     try
    //     {
    //         var result = 0;
    //         using var connection = _ds.OpenConnection();
    //         {
    //             var cmd = new NpgsqlCommand();
    //             cmd.Connection = connection;
    //             cmd.CommandText = $"INSERT INTO public.shipping (order_id, shipper, goods_status, status) VALUES (@order_id, @shipper, @customer, @restaurant_status)";
    //             cmd.Parameters.AddWithValue("order_id", shipping.OrderId);
    //             cmd.Parameters.AddWithValue("shipper", shipping.Shipper);
    //             cmd.Parameters.AddWithValue("goods_status", shipping.GoodsStatus);
    //             cmd.Parameters.AddWithValue("status", shipping.Status);
    //             cmd.Prepare();
    //             result = (int)await cmd.ExecuteNonQueryAsync();
    //         }

    //         return true;
    //     }
    //     catch (System.Exception ex)
    //     {
    //         Console.WriteLine($"Ex: {ex.Message} \n {ex.StackTrace}");
    //         throw ex;
    //     }
    // }
    // public async Task<bool> UpdateShipper(int orderId, string shipper)
    // {
    //     try
    //     {
    //         using var connection = _ds.OpenConnection();
    //         {
    //             var cmd = new NpgsqlCommand();
    //             cmd.Connection = connection;
    //             cmd.CommandText = $"UPDATE public.shipping SET shipper = @shipper WHERE order_id = @order_id";
    //             cmd.Parameters.AddWithValue("order_id", orderId);
    //             cmd.Parameters.AddWithValue("shipper", shipper);
    //             cmd.Prepare();
    //             await cmd.ExecuteNonQueryAsync();
    //         }

    //         return true;
    //     }
    //     catch (System.Exception ex)
    //     {
    //         Console.WriteLine($"Ex: {ex.Message} \n {ex.StackTrace}");
    //         throw ex;
    //     }
    // }

    // public async Task<bool> UpdateRestaurantStatus(int orderId, string restaurantStatus)
    // {
    //     try
    //     {
    //         using var connection = _ds.OpenConnection();
    //         {
    //             var cmd = new NpgsqlCommand();
    //             cmd.Connection = connection;
    //             cmd.CommandText = $"UPDATE public.shipping SET restaurant_status = @restaurant_status WHERE order_id = @order_id";
    //             cmd.Parameters.AddWithValue("order_id", orderId);
    //             cmd.Parameters.AddWithValue("restaurant_status", restaurantStatus);
    //             cmd.Prepare();
    //             await cmd.ExecuteNonQueryAsync();
    //         }

    //         return true;
    //     }
    //     catch (System.Exception ex)
    //     {
    //         Console.WriteLine($"Ex: {ex.Message} \n {ex.StackTrace}");
    //         throw ex;
    //     }
    // }
}