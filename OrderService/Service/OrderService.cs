using System.Data;
using Common;
using Newtonsoft.Json;
using Npgsql;
using OrderService.Domain;
using OrderService.Messing;

namespace OrderService.Service;
public class OrderService : IOrderService
{
    private readonly NpgsqlDataSource _ds;

    public OrderService(IConfiguration configuration)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(configuration.GetConnectionString("DB"));
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
                        Shipper = reader["shipper"] as string,
                    });
                }

            }
            return result.FirstOrDefault();
        }
        catch (System.Exception ex)
        {
            Console.WriteLine($"Ex: {ex.Message} \n {ex.StackTrace}");
            throw ex;
        }
    }

    public async Task<int> CreateOrder(Order order)
    {
        NpgsqlTransaction transaction = null;
        try
        {
            using var connection = _ds.OpenConnection();
            {
                transaction = connection.BeginTransaction();
                var cmd1 = new NpgsqlCommand();
                cmd1.Connection = connection;
                cmd1.CommandText = $"INSERT INTO public.order (order_by, product, quantity, total_money, status) VALUES (@order_by, @product, @quantity, @total_money, @status) RETURNING id;";
                cmd1.Parameters.AddWithValue("order_by", order.OrderBy);
                cmd1.Parameters.AddWithValue("product", order.Product);
                cmd1.Parameters.AddWithValue("quantity", order.Quantity);
                cmd1.Parameters.AddWithValue("total_money", order.TotalMoney);
                cmd1.Parameters.AddWithValue("status", order.Status);
                cmd1.Prepare();
                order.Id = (int)cmd1.ExecuteScalar();

                var message = new CreateOrderMessage()
                {
                    Id = order.Id,
                    OrderBy = order.OrderBy,
                    Product = order.Product,
                    Quantity = order.Quantity,
                    Shipper = string.Empty,
                    TotalMoney = order.TotalMoney
                };
                var cmd2 = new NpgsqlCommand();
                cmd2.Connection = connection;
                cmd2.Transaction = transaction;
                cmd2.CommandText = $"INSERT INTO public.event (type, data, status) VALUES (@type, @data, @status)";
                cmd2.Parameters.AddWithValue("type", CoreConstant.Topic.ORDER_CREATE_PENDING);
                cmd2.Parameters.AddWithValue("data", JsonConvert.SerializeObject(message));
                cmd2.Parameters.AddWithValue("status", CoreConstant.EventStatus.NEW);
                cmd2.Prepare();
                await cmd2.ExecuteNonQueryAsync();

                transaction.Commit();
            }

            return order.Id;
        }
        catch (System.Exception ex)
        {
            if (transaction != null) transaction.Rollback();
            Console.WriteLine($"Ex: {ex.Message} \n {ex.StackTrace}");
            throw ex;
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
                cmd.CommandText = $"UPDATE public.order SET status= @staus WHERE id= @id";
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
            throw ex;
        }
    }

    public async Task<int> UpdateOrderShipper(int orderId, string shipper)
    {
        try
        {
            using var connection = _ds.OpenConnection();
            {
                var cmd = new NpgsqlCommand();
                cmd.Connection = connection;
                cmd.CommandText = $"UPDATE public.order SET shipper= @shipper WHERE id= @id";
                cmd.Parameters.AddWithValue("id", orderId);
                cmd.Parameters.AddWithValue("shipper", shipper);
                cmd.Prepare();
                await cmd.ExecuteNonQueryAsync();
            }

            return orderId;
        }
        catch (System.Exception ex)
        {
            Console.WriteLine($"Ex: {ex.Message} \n {ex.StackTrace}");
            throw ex;
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
            throw ex;
        }
    }

}