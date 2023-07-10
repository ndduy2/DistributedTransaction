using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;
using DeliveryService.Domain;

namespace DeliveryService.Service;
public class ShippingService : IShippingService
{
    private readonly NpgsqlDataSource _ds;

    public ShippingService(IConfiguration configuration)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(configuration.GetConnectionString("Orderdb"));
        _ds = dataSourceBuilder.Build();
    }

    public async Task<string> FindShipper(int cf)
    {
        try
        {
            if (cf % 2 == 0)
            {
                return "TuNT";
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

    public async Task<Shipping> GetByOrderId(int orderId)
    {
        try
        {
            List<Shipping> result = new List<Shipping>();
            using var connection = _ds.OpenConnection();
            {
                var cmd = new NpgsqlCommand();
                cmd.Connection = connection;
                cmd.CommandText = $"SELECT * FROM public.shipping WHERE order_id = @order_id";
                cmd.Parameters.AddWithValue("order_id", orderId);
                cmd.Prepare();
                NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Add(new Shipping()
                    {
                        OrderId = (int)reader["order_id"],
                        Shipper = reader["shipper"] as string,
                        Customer = reader["customer"] as string,
                        RestaurantStatus = reader["restaurant_status"] as string,
                    });
                }

            }
            return result.FirstOrDefault();
        }
        catch (System.Exception ex)
        {
            Console.WriteLine($"Ex: {ex.Message} \n {ex.StackTrace}");
            return null;
        }
    }

    public async Task<bool> CreateShipping(Shipping shipping)
    {
        try
        {
            var result = 0;
            using var connection = _ds.OpenConnection();
            {
                var cmd = new NpgsqlCommand();
                cmd.Connection = connection;
                cmd.CommandText = $"INSERT INTO public.shipping (order_id, shipper, customer, restaurant_status) VALUES (@order_id, @shipper, @customer, @restaurant_status)";
                cmd.Parameters.AddWithValue("order_id", shipping.OrderId);
                cmd.Parameters.AddWithValue("shipper", shipping.Shipper);
                cmd.Parameters.AddWithValue("customer", shipping.Customer);
                cmd.Parameters.AddWithValue("restaurant_status", shipping.RestaurantStatus);
                cmd.Prepare();
                result = (int)await cmd.ExecuteNonQueryAsync();
            }

            return true;
        }
        catch (System.Exception ex)
        {
            Console.WriteLine($"Ex: {ex.Message} \n {ex.StackTrace}");
            return false;
        }
    }
    public async Task<bool> UpdateShipper(int orderId, string shipper)
    {
        try
        {
            using var connection = _ds.OpenConnection();
            {
                var cmd = new NpgsqlCommand();
                cmd.Connection = connection;
                cmd.CommandText = $"UPDATE public.shipping SET shipper = @shipper WHERE order_id = @order_id";
                cmd.Parameters.AddWithValue("order_id", orderId);
                cmd.Parameters.AddWithValue("shipper", shipper);
                cmd.Prepare();
                await cmd.ExecuteNonQueryAsync();
            }

            return true;
        }
        catch (System.Exception ex)
        {
            Console.WriteLine($"Ex: {ex.Message} \n {ex.StackTrace}");
            return false;
        }
    }

    public async Task<bool> UpdateRestaurantStatus(int orderId, string restaurantStatus)
    {
        try
        {
            using var connection = _ds.OpenConnection();
            {
                var cmd = new NpgsqlCommand();
                cmd.Connection = connection;
                cmd.CommandText = $"UPDATE public.shipping SET restaurant_status = @restaurant_status WHERE order_id = @order_id";
                cmd.Parameters.AddWithValue("order_id", orderId);
                cmd.Parameters.AddWithValue("restaurant_status", restaurantStatus);
                cmd.Prepare();
                await cmd.ExecuteNonQueryAsync();
            }

            return true;
        }
        catch (System.Exception ex)
        {
            Console.WriteLine($"Ex: {ex.Message} \n {ex.StackTrace}");
            return false;
        }
    }
}