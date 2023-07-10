using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;
using PaymentService.Domain;

namespace PaymentService.Service;
public class PaymentLogService : IPaymentLogService
{
    private readonly NpgsqlDataSource _ds;

    public PaymentLogService(IConfiguration configuration)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(configuration.GetConnectionString("Orderdb"));
        _ds = dataSourceBuilder.Build();
    }

    public async Task<bool> Create(PaymentLog model)
    {
        try
        {
            using var connection = _ds.OpenConnection();
            {
                var cmd = new NpgsqlCommand();
                cmd.Connection = connection;
                cmd.CommandText = $"INSERT INTO public.payment_log (order_id, paid) VALUES (@order_id, @paid)";
                cmd.Parameters.AddWithValue("order_id", model.OrderId);
                cmd.Parameters.AddWithValue("paid", model.Paid);
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

    public async Task<bool> UpdateStatus(int orderId, bool paid)
    {
        try
        {
            using var connection = _ds.OpenConnection();
            {
                var cmd = new NpgsqlCommand();
                cmd.Connection = connection;
                cmd.CommandText = $"UPDATE public.payment_log set paid = @paid WHERE order_id = @order_id";
                cmd.Parameters.AddWithValue("order_id", orderId);
                cmd.Parameters.AddWithValue("paid", paid);
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

    public async Task<PaymentLog> GetByOrderId(int orderId)
    {
        try
        {
            List<PaymentLog> result = new List<PaymentLog>();
            using var connection = _ds.OpenConnection();
            {
                var cmd = new NpgsqlCommand();
                cmd.Connection = connection;
                cmd.CommandText = $"SELECT * FROM public.payment_log WHERE order_id=@order_id";
                cmd.Parameters.AddWithValue("order_id", orderId);
                cmd.Prepare();
                NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Add(new PaymentLog()
                    {
                        OrderId = (int)reader["order_id"],
                        Paid = (bool)reader["paid"],
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
}