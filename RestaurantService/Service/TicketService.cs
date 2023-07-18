using System.Data;
using Common;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Npgsql;
using RestaurantService.Domain;
using RestaurantService.Messing;

namespace RestaurantService.Service;
public class TicketService : ITicketService
{
    private readonly NpgsqlDataSource _ds;

    public TicketService(IConfiguration configuration)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(configuration.GetConnectionString("DB"));
        _ds = dataSourceBuilder.Build();
    }

    public async Task<Ticket> GetByOrderId(int orderId)
    {
        try
        {
            List<Ticket> result = new List<Ticket>();
            using var connection = _ds.OpenConnection();
            {
                var cmd = new NpgsqlCommand();
                cmd.Connection = connection;
                cmd.CommandText = $"SELECT * FROM public.ticket WHERE order_id=@order_id";
                cmd.Parameters.AddWithValue("order_id", orderId);
                cmd.Prepare();
                NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Add(new Ticket()
                    {
                        OrderId = (int)reader["order_id"],
                        Status = reader["status"] as string,
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

    public async Task<int> UpdateStatus(int orderId, string status)
    {
        try
        {
            using var connection = _ds.OpenConnection();
            {
                var cmd = new NpgsqlCommand();
                cmd.Connection = connection;
                cmd.CommandText = $"UPDATE public.ticket SET status= @status WHERE order_id= @order_id";
                cmd.Parameters.AddWithValue("order_id", orderId);
                cmd.Parameters.AddWithValue("status", status);
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

    public async Task<int> DeleteByOrderId(int orderId)
    {
        try
        {
            using var connection = _ds.OpenConnection();
            {
                var cmd = new NpgsqlCommand();
                cmd.Connection = connection;
                cmd.CommandText = $"DELETE FROM public.ticket WHERE order_id=@order_id";
                cmd.Parameters.AddWithValue("order_id", orderId);
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

    public async Task Approve(CreateOrderMessage model)
    {
        NpgsqlTransaction transaction = null;
        try
        {
            using var connection = _ds.OpenConnection();
            {
                transaction = connection.BeginTransaction();
                var cmd1 = new NpgsqlCommand();
                cmd1.Connection = connection;
                cmd1.CommandText = $"UPDATE public.ticket SET status= @status WHERE order_id= @order_id";
                cmd1.Parameters.AddWithValue("order_id", model.Id);
                cmd1.Parameters.AddWithValue("status", CoreConstant.TicketStatus.APPROVED);
                cmd1.Prepare();
                await cmd1.ExecuteNonQueryAsync();

                var cmd2 = new NpgsqlCommand();
                cmd2.Connection = connection;
                cmd2.Transaction = transaction;
                cmd2.CommandText = $"UPDATE public.inventory SET status= @status WHERE product= @product";
                cmd2.Parameters.AddWithValue("product", model.Product);
                cmd2.Parameters.AddWithValue("status", CoreConstant.InventoryStatus.AVAIABLE);
                cmd2.Prepare();
                await cmd2.ExecuteNonQueryAsync();

                var cmd3 = new NpgsqlCommand();
                cmd3.Connection = connection;
                cmd3.Transaction = transaction;
                cmd3.CommandText = $"INSERT INTO public.event (type, data, status) VALUES (@type, @data, @status)";
                cmd3.Parameters.AddWithValue("type", CoreConstant.Topic.TICKET_APPROVED);
                cmd3.Parameters.AddWithValue("data", JsonConvert.SerializeObject(model));
                cmd3.Parameters.AddWithValue("status", CoreConstant.EventStatus.NEW);
                cmd3.Prepare();
                await cmd3.ExecuteNonQueryAsync();

                transaction.Commit();
            }

            return;
        }
        catch (System.Exception ex)
        {
            if (transaction != null) transaction.Rollback();
            Console.WriteLine($"Ex: {ex.Message} \n {ex.StackTrace}");
            throw ex;
        }
    }

    public async Task Done(CreateOrderMessage model)
    {
        NpgsqlTransaction transaction = null;
        try
        {
            using var connection = _ds.OpenConnection();
            {
                transaction = connection.BeginTransaction();
                var cmd1 = new NpgsqlCommand();
                cmd1.Connection = connection;
                cmd1.CommandText = $"UPDATE public.ticket SET status= @status WHERE order_id= @order_id";
                cmd1.Parameters.AddWithValue("order_id", model.Id);
                cmd1.Parameters.AddWithValue("status", CoreConstant.TicketStatus.DONE);
                cmd1.Prepare();
                await cmd1.ExecuteNonQueryAsync();

                var cmd2 = new NpgsqlCommand();
                cmd2.Connection = connection;
                cmd2.Transaction = transaction;
                cmd2.CommandText = $"INSERT INTO public.event (type, data, status) VALUES (@type, @data, @status)";
                cmd2.Parameters.AddWithValue("type", CoreConstant.Topic.TICKET_DONE);
                cmd2.Parameters.AddWithValue("data", JsonConvert.SerializeObject(model));
                cmd2.Parameters.AddWithValue("status", CoreConstant.EventStatus.NEW);
                cmd2.Prepare();
                await cmd2.ExecuteNonQueryAsync();

                transaction.Commit();
            }

            return;
        }
        catch (System.Exception ex)
        {
            if (transaction != null) transaction.Rollback();
            Console.WriteLine($"Ex: {ex.Message} \n {ex.StackTrace}");
            throw ex;
        }
    }
}