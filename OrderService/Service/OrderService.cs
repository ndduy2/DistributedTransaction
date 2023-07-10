using System.Data;
using Npgsql;
using OrderService.Domain;

namespace OrderService.Service;
public class OrderService : IOrderService
{
    private readonly NpgsqlDataSource _ds;

    public OrderService(IConfiguration configuration)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(configuration.GetConnectionString("Orderdb"));
        _ds = dataSourceBuilder.Build();
    }

    public async Task<Order> GetOneById(int orderId)
    {
        try
        {
            List<Order> result = new List<Order>();
            using var connection = _ds.OpenConnection();
            {
                var cmd = new NpgsqlCommand();
                cmd.Connection = connection;
                cmd.CommandText = $"SELECT * FROM public.order WHERE id=@id";
                cmd.Parameters.AddWithValue("id", orderId);
                cmd.Prepare();
                NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Add(new Order()
                    {
                        Id = (int)reader["id"],
                        OrderBy = reader["order_by"] as string,
                        Product = reader["product"] as string,
                        Quantity = (int)reader["quantity"],
                        TotalMoney = (int)reader["total_money"],
                        Status = reader["status"] as string,
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

    public async Task<int> CreateOrder(Order order)
    {
        try
        {
            var result = 0;
            using var connection = _ds.OpenConnection();
            {
                var cmd = new NpgsqlCommand();
                cmd.Connection = connection;
                cmd.CommandText = $"INSERT INTO public.order (order_by, product, quantity, total_money, status) VALUES (@order_by, @product, @quantity, @total_money, @status) RETURNING id;";
                cmd.Parameters.AddWithValue("order_by", order.OrderBy);
                cmd.Parameters.AddWithValue("product", order.Product);
                cmd.Parameters.AddWithValue("quantity", order.Quantity);
                cmd.Parameters.AddWithValue("total_money", order.TotalMoney);
                cmd.Parameters.AddWithValue("status", "CREATED");
                cmd.Prepare();
                result = (int)cmd.ExecuteScalar();
            }

            return result;
        }
        catch (System.Exception ex)
        {
            Console.WriteLine($"Ex: {ex.Message} \n {ex.StackTrace}");
            return 0;
        }
    }

    public async Task<int> UpdateOrder(int orderId, string status)
    {
        try
        {
            using var connection = _ds.OpenConnection();
            {
                var cmd = new NpgsqlCommand();
                cmd.Connection = connection;
                cmd.CommandText = $"UPDATE public.order SET status = @staus WHERE id=@id";
                cmd.Parameters.AddWithValue("id", orderId);
                cmd.Parameters.AddWithValue("staus", status);
                cmd.Prepare();
                await cmd.ExecuteNonQueryAsync();
            }

            return orderId;
        }
        catch (System.Exception ex)
        {
            Console.WriteLine($"Ex: {ex.Message} \n {ex.StackTrace}");
            return 0;
        }
    }

    public async Task<int> DeleteOrder(int orderId)
    {
        try
        {
            using var connection = _ds.OpenConnection();
            {
                var cmd = new NpgsqlCommand();
                cmd.Connection = connection;
                cmd.CommandText = $"DELETE FROM public.order WHERE id=@id";
                cmd.Parameters.AddWithValue("id", orderId);
                cmd.Prepare();
                await cmd.ExecuteNonQueryAsync();
            }

            return orderId;
        }
        catch (System.Exception ex)
        {
            Console.WriteLine($"Ex: {ex.Message} \n {ex.StackTrace}");
            return 0;
        }
    }

}