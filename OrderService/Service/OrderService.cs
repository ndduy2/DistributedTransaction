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
                NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Add(new Order()
                    {
                        Id = (int)reader["id"],
                        OrderBy = reader["order_by"] as string,
                        Amount = (int)reader["amount"],
                        Status = reader["status"] as string,
                    });
                }

            }
            return result.FirstOrDefault();
        }
        catch (System.Exception ex)
        {
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
                cmd.CommandText = $"INSERT INTO public.order (order_by, amount, status) VALUES (@orderBy, @amount, @status) RETURNING id;";
                cmd.Parameters.AddWithValue("orderBy", order.OrderBy);
                cmd.Parameters.AddWithValue("amount", order.Amount);
                cmd.Parameters.AddWithValue("status", "CREATED");
                result = (int)cmd.ExecuteScalar();
            }

            return result;
        }
        catch (System.Exception ex)
        {
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
                await cmd.ExecuteNonQueryAsync();
            }

            return orderId;
        }
        catch (System.Exception ex)
        {
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
                await cmd.ExecuteNonQueryAsync();
            }

            return orderId;
        }
        catch (System.Exception ex)
        {
            return 0;
        }
    }

}