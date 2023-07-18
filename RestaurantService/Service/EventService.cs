using System.Data;
using Common;
using Microsoft.Extensions.Configuration;
using Npgsql;
using RestaurantService.Domain;

namespace RestaurantService.Service;
public class EventService : IEventService
{
    private readonly NpgsqlDataSource _ds;

    public EventService(IConfiguration configuration)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(configuration.GetConnectionString("DB"));
        _ds = dataSourceBuilder.Build();
    }

    public async Task<int> CreateEvent(Event ev)
    {
        try
        {
            var result = 0;
            using var connection = _ds.OpenConnection();
            {
                var cmd1 = new NpgsqlCommand();
                cmd1.Connection = connection;
                cmd1.CommandText = $"INSERT INTO public.event (type, data, status) VALUES (@type, @data, @status) RETURNING id;";
                cmd1.Parameters.AddWithValue("type", ev.Type);
                cmd1.Parameters.AddWithValue("data", ev.Data);
                cmd1.Parameters.AddWithValue("status", CoreConstant.EventStatus.NEW);
                cmd1.Prepare();
                result = (int)cmd1.ExecuteScalar();
            }

            return result;
        }
        catch (System.Exception ex)
        {
            Console.WriteLine($"Ex: {ex.Message} \n {ex.StackTrace}");
            throw ex;
        }
    }

    public async Task<Event> GetNextPendingEvent()
    {
        try
        {
            List<Event> result = new List<Event>();
            using var connection = _ds.OpenConnection();
            {
                var cmd = new NpgsqlCommand();
                cmd.Connection = connection;
                cmd.CommandText = $"SELECT * FROM public.event WHERE status = @status ORDER BY id ASC LIMIT 1";
                cmd.Parameters.AddWithValue("status", CoreConstant.EventStatus.NEW);
                cmd.Prepare();
                NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Add(new Event()
                    {
                        Id = (int)reader["id"],
                        Type = reader["type"] as string,
                        Data = reader["data"] as string,
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

    public async Task<bool> UpdateEventStatus(int id, string status)
    {
        try
        {
            using var connection = _ds.OpenConnection();
            {
                var cmd = new NpgsqlCommand();
                cmd.Connection = connection;
                cmd.CommandText = $"UPDATE public.event SET status = @status WHERE id = @id";
                cmd.Parameters.AddWithValue("id", id);
                cmd.Parameters.AddWithValue("status", status);
                cmd.Prepare();
                await cmd.ExecuteNonQueryAsync();
            }

            return true;
        }
        catch (System.Exception ex)
        {
            Console.WriteLine($"Ex: {ex.Message} \n {ex.StackTrace}");
            throw ex;
        }
    }
}