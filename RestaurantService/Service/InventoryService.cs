using System.Data;
using Common;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Npgsql;
using RestaurantService.Domain;
using RestaurantService.Messing;

namespace RestaurantService.Service;
public class InventoryService : IInventoryService
{
    private readonly NpgsqlDataSource _ds;

    public InventoryService(IConfiguration configuration)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(configuration.GetConnectionString("DB"));
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
                        Status = reader["status"] as string,
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

    public async Task UpdateStock(CreateOrderMessage model, int stock, string status = CoreConstant.InventoryStatus.LOCKING)
    {
        NpgsqlTransaction transaction = null;
        try
        {
            using var connection = _ds.OpenConnection();
            {
                transaction = connection.BeginTransaction();
                var cmd1 = new NpgsqlCommand();
                cmd1.Connection = connection;
                cmd1.Transaction = transaction;
                cmd1.CommandText = $"UPDATE public.inventory SET stock= @stock, status= @status WHERE product= @product";
                cmd1.Parameters.AddWithValue("product", model.Product);
                cmd1.Parameters.AddWithValue("stock", stock);
                cmd1.Parameters.AddWithValue("status", status);
                cmd1.Prepare();
                await cmd1.ExecuteNonQueryAsync();

                var cmd2 = new NpgsqlCommand();
                cmd2.Connection = connection;
                cmd2.Transaction = transaction;
                cmd2.CommandText = $"INSERT INTO public.ticket (order_id, status) VALUES (@order_id, @status)";
                cmd2.Parameters.AddWithValue("order_id", model.Id);
                cmd2.Parameters.AddWithValue("status", CoreConstant.TicketStatus.CREATE_PENDING);
                cmd2.Prepare();
                await cmd2.ExecuteNonQueryAsync();

                var cmd3 = new NpgsqlCommand();
                cmd3.Connection = connection;
                cmd3.Transaction = transaction;
                cmd3.CommandText = $"INSERT INTO public.event (type, data, status) VALUES (@type, @data, @status)";
                cmd3.Parameters.AddWithValue("type", CoreConstant.Topic.TICKET_CREATE_PENDING);
                cmd3.Parameters.AddWithValue("data", JsonConvert.SerializeObject(model));
                cmd3.Parameters.AddWithValue("status", CoreConstant.EventStatus.NEW);
                cmd3.Prepare();
                await cmd3.ExecuteNonQueryAsync();

                await transaction.CommitAsync();
            }

            return;
        }
        catch (System.Exception ex)
        {
            if (transaction != null) transaction.Rollback();
            Console.WriteLine($"Ex: {ex.Message} \n {ex.StackTrace}");
            return;
        }
    }

    public async Task RevertStock(string product, int stock)
    {
        try
        {
            using var connection = _ds.OpenConnection();
            {
                var cmd1 = new NpgsqlCommand();
                cmd1.Connection = connection;
                cmd1.CommandText = $"UPDATE public.inventory SET stock= @stock, status= @status WHERE product= @product";
                cmd1.Parameters.AddWithValue("product", product);
                cmd1.Parameters.AddWithValue("stock", stock);
                cmd1.Parameters.AddWithValue("status", CoreConstant.InventoryStatus.AVAIABLE);
                cmd1.Prepare();
                await cmd1.ExecuteNonQueryAsync();

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