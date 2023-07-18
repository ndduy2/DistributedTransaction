using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;
using OrderService.Domain;

namespace OrderService.Service;
public class OrderHistoryService : IOrderHistoryService
{
    private readonly NpgsqlDataSource _ds;

    public OrderHistoryService(IConfiguration configuration)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(configuration.GetConnectionString("DB"));
        _ds = dataSourceBuilder.Build();
    }

    public async Task<bool> Create(OrderHistory model)
    {
        try
        {
            using var connection = _ds.OpenConnection();
            {
                var cmd1 = new NpgsqlCommand();
                cmd1.Connection = connection;
                cmd1.CommandText = $"INSERT INTO public.order_history (order_id, ts, status) VALUES (@order_id, @ts, @status)";
                cmd1.Parameters.AddWithValue("order_id", model.OrderId);
                cmd1.Parameters.AddWithValue("ts", model.Ts);
                cmd1.Parameters.AddWithValue("status", model.Status);
                cmd1.Prepare();
                cmd1.ExecuteNonQuery();
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