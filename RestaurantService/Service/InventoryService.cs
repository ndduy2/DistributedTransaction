using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;
using RestaurantService.Domain;

namespace RestaurantService.Service;
public class InventoryService : IInventoryService
{
    private readonly NpgsqlDataSource _ds;

    public InventoryService(IConfiguration configuration)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(configuration.GetConnectionString("Orderdb"));
        _ds = dataSourceBuilder.Build();
    }

    public async Task<Inventory> GetAmount(string product)
    {
        try
        {
            List<Inventory> result = new List<Inventory>();
            using var connection = _ds.OpenConnection();
            {
                var cmd = new NpgsqlCommand();
                cmd.Connection = connection;
                cmd.CommandText = $"SELECT * FROM public.inventory WHERE product=@product";
                cmd.Parameters.AddWithValue("product", product);
                cmd.Prepare();
                NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Add(new Inventory()
                    {
                        Product = reader["product"] as string,
                        Stock = (int)reader["stock"],
                    });
                }

            }

            return result.FirstOrDefault();
        }
        catch (System.Exception ex)
        {
            Console.WriteLine($"Ex: {ex.Message} \n {ex.StackTrace}");
            return new Inventory();
        }
    }

    public async Task UpdateStock(string product, int stock, int? orderId = null)
    {
        try
        {
            using var connection = _ds.OpenConnection();
            {
                var transaction = connection.BeginTransaction();
                var cmd1 = new NpgsqlCommand();
                cmd1.Connection = connection;
                cmd1.CommandText = $"UPDATE public.inventory SET stock = @stock WHERE product=@product";
                cmd1.Parameters.AddWithValue("product", product);
                cmd1.Parameters.AddWithValue("stock", stock);
                cmd1.Prepare();
                await cmd1.ExecuteNonQueryAsync();

                if (orderId != null)
                {
                    var cmd2 = new NpgsqlCommand();
                    cmd2.Connection = connection;
                    cmd2.Transaction = transaction;
                    cmd2.CommandText = $"INSERT INTO public.restaurant_log (order_id, is_cooking) VALUES (@order_id, @is_cooking)";
                    cmd2.Parameters.AddWithValue("order_id", orderId);
                    cmd2.Parameters.AddWithValue("is_cooking", true);
                    cmd2.Prepare();
                    await cmd2.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
            }

            return;
        }
        catch (System.Exception ex)
        {
            Console.WriteLine($"Ex: {ex.Message} \n {ex.StackTrace}");
            return;
        }
    }
}