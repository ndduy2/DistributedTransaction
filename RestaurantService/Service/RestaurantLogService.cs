using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;
using RestaurantService.Domain;

namespace RestaurantService.Service;
public class RestaurantLogService : IRestaurantLogService
{
    private readonly NpgsqlDataSource _ds;

    public RestaurantLogService(IConfiguration configuration)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(configuration.GetConnectionString("Orderdb"));
        _ds = dataSourceBuilder.Build();
    }

    public async Task<bool> Create(RestaurantLog model)
    {
        try
        {
            using var connection = _ds.OpenConnection();
            {
                var cmd = new NpgsqlCommand();
                cmd.Connection = connection;
                cmd.CommandText = $"INSERT INTO public.restaurant_log (order_id, is_cooking) VALUES (@order_id, @is_cooking)";
                cmd.Parameters.AddWithValue("order_id", model.OrderId);
                cmd.Parameters.AddWithValue("is_cooking", model.IsCooking);
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

    public async Task<bool> UpdateStatus(int orderId, bool isCooking)
    {
        try
        {
            using var connection = _ds.OpenConnection();
            {
                var cmd = new NpgsqlCommand();
                cmd.Connection = connection;
                cmd.CommandText = $"UPDATE public.restaurant_log set is_cooking = @is_cooking WHERE order_id = @order_id";
                cmd.Parameters.AddWithValue("order_id", orderId);
                cmd.Parameters.AddWithValue("is_cooking", isCooking);
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

    public async Task<RestaurantLog> GetByOrderId(int orderId)
    {
        try
        {
            List<RestaurantLog> result = new List<RestaurantLog>();
            using var connection = _ds.OpenConnection();
            {
                var cmd = new NpgsqlCommand();
                cmd.Connection = connection;
                cmd.CommandText = $"SELECT * FROM public.restaurant_log WHERE order_id=@order_id";
                cmd.Parameters.AddWithValue("order_id", orderId);
                cmd.Prepare();
                NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Add(new RestaurantLog()
                    {
                        OrderId = (int)reader["order_id"],
                        IsCooking = (bool)reader["is_cooking"],
                    });
                }

            }

            return result.FirstOrDefault();
        }
        catch (System.Exception ex)
        {
            return new RestaurantLog();
        }
    }
}