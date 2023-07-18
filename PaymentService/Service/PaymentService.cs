using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;
using PaymentService.Domain;

namespace PaymentService.Service;
public class PaymentService : IPaymentService
{
    private readonly NpgsqlDataSource _ds;

    public PaymentService(IConfiguration configuration)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(configuration.GetConnectionString("DB"));
        _ds = dataSourceBuilder.Build();
    }
    public async Task<Payment> GetByOrderId(int orderId)
    {
        try
        {
            List<Payment> result = new List<Payment>();
            using var connection = _ds.OpenConnection();
            {
                var cmd = new NpgsqlCommand();
                cmd.Connection = connection;
                cmd.CommandText = $"SELECT * FROM public.payment WHERE order_id = @order_id LIMIT 1";
                cmd.Parameters.AddWithValue("order_id", orderId);
                cmd.Prepare();
                NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Add(new Payment()
                    {
                        OrderId = (int)reader["order_id"],
                        Account = reader["account"] as string,
                        Amount = (int)reader["amount"],
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
}