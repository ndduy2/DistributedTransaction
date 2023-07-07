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
            return null;
        }
    }

    public async Task<bool> UpdateBalance(string account, int balance)
    {
        try
        {
            using var connection = _ds.OpenConnection();
            {
                var cmd = new NpgsqlCommand();
                cmd.Connection = connection;
                cmd.CommandText = $"UPDATE public.account_balance SET balance = @balance WHERE account=@account";
                cmd.Parameters.AddWithValue("account", account);
                cmd.Parameters.AddWithValue("balance", balance);
                await cmd.ExecuteNonQueryAsync();
            }

            return true;
        }
        catch (System.Exception ex)
        {
            return false;
        }
    }
}