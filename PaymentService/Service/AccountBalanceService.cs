using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;
using PaymentService.Domain;

namespace PaymentService.Service;
public class AccountBalanceService : IAccountBalanceService
{
    private readonly NpgsqlDataSource _ds;

    public AccountBalanceService(IConfiguration configuration)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(configuration.GetConnectionString("Orderdb"));
        _ds = dataSourceBuilder.Build();
    }

    public async Task<AccountBalance> GetByAccount(string account)
    {
        try
        {
            List<AccountBalance> result = new List<AccountBalance>();
            using var connection = _ds.OpenConnection();
            {
                var cmd = new NpgsqlCommand();
                cmd.Connection = connection;
                cmd.CommandText = $"SELECT * FROM public.account_balance WHERE account=@account";
                cmd.Parameters.AddWithValue("account", account);
                cmd.Prepare();
                NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Add(new AccountBalance()
                    {
                        Account = reader["account"] as string,
                        Balance = (int)reader["Balance"],
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

    public async Task<bool> UpdateBalance(string account, int balance, int? orderId = null)
    {
        try
        {
            using var connection = _ds.OpenConnection();
            {

                var transaction = connection.BeginTransaction();
                var cmd1 = new NpgsqlCommand();
                cmd1.Connection = connection;
                cmd1.Transaction = transaction;
                cmd1.CommandText = $"UPDATE public.account_balance SET balance = @balance WHERE account=@account";
                cmd1.Parameters.AddWithValue("account", account);
                cmd1.Parameters.AddWithValue("balance", balance);
                cmd1.Prepare();
                await cmd1.ExecuteNonQueryAsync();

                if (orderId != null)
                {
                    var cmd2 = new NpgsqlCommand();
                    cmd2.Connection = connection;
                    cmd2.Transaction = transaction;
                    cmd2.CommandText = $"INSERT INTO public.payment_log (order_id, paid) VALUES (@order_id, @paid)";
                    cmd2.Parameters.AddWithValue("order_id", orderId);
                    cmd2.Parameters.AddWithValue("paid", true);
                    cmd2.Prepare();
                    await cmd2.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
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