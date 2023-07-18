using System.Data;
using Common;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Npgsql;
using PaymentService.Domain;
using PaymentService.Messing;

namespace PaymentService.Service;
public class AccountBalanceService : IAccountBalanceService
{
    private readonly NpgsqlDataSource _ds;

    public AccountBalanceService(IConfiguration configuration)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(configuration.GetConnectionString("DB"));
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
            throw ex;
        }
    }

    public async Task<bool> UpdateBalance(CreateOrderMessage model, int balance)
    {
        NpgsqlTransaction transaction = null;
        try
        {
            using var connection = _ds.OpenConnection();
            {

                transaction = connection.BeginTransaction();

                var cmd1 = new NpgsqlCommand();
                cmd1.Connection = connection;
                cmd1.Transaction = transaction;
                cmd1.CommandText = $"UPDATE public.account_balance SET balance = @balance WHERE account= @account";
                cmd1.Parameters.AddWithValue("account", model.OrderBy);
                cmd1.Parameters.AddWithValue("balance", balance);
                cmd1.Prepare();
                await cmd1.ExecuteNonQueryAsync();

                var cmd2 = new NpgsqlCommand();
                cmd2.Connection = connection;
                cmd2.Transaction = transaction;
                cmd2.CommandText = $"INSERT INTO public.payment (order_id, account, amount, status) VALUES (@order_id, @account, @amount, @status)";
                cmd2.Parameters.AddWithValue("order_id", model.Id);
                cmd2.Parameters.AddWithValue("account", model.OrderBy);
                cmd2.Parameters.AddWithValue("amount", model.TotalMoney);
                cmd2.Parameters.AddWithValue("status", CoreConstant.PaymentStatus.PAID);
                cmd2.Prepare();
                await cmd2.ExecuteNonQueryAsync();

                var cmd3 = new NpgsqlCommand();
                cmd3.Connection = connection;
                cmd3.Transaction = transaction;
                cmd3.CommandText = $"INSERT INTO public.event (type, data, status) VALUES (@type, @data, @status)";
                cmd3.Parameters.AddWithValue("type", CoreConstant.Topic.PAYMENT_SUCCESS);
                cmd3.Parameters.AddWithValue("data", JsonConvert.SerializeObject(model));
                cmd3.Parameters.AddWithValue("status", CoreConstant.EventStatus.NEW);
                cmd3.Prepare();
                await cmd3.ExecuteNonQueryAsync();

                await transaction.CommitAsync();
            }

            return true;
        }
        catch (System.Exception ex)
        {
            if (transaction != null) transaction.Rollback();
            Console.WriteLine($"Ex: {ex.Message} \n {ex.StackTrace}");
            throw ex;
        }
    }

    public async Task<bool> RevertBalance(CreateOrderMessage model, int balance)
    {
        NpgsqlTransaction transaction = null;
        try
        {
            using var connection = _ds.OpenConnection();
            {

                transaction = connection.BeginTransaction();

                var cmd1 = new NpgsqlCommand();
                cmd1.Connection = connection;
                cmd1.Transaction = transaction;
                cmd1.CommandText = $"UPDATE public.account_balance SET balance = @balance WHERE account= @account";
                cmd1.Parameters.AddWithValue("account", model.OrderBy);
                cmd1.Parameters.AddWithValue("balance", balance);
                cmd1.Prepare();
                await cmd1.ExecuteNonQueryAsync();

                var cmd2 = new NpgsqlCommand();
                cmd2.Connection = connection;
                cmd2.Transaction = transaction;
                cmd2.CommandText = $"UPDATE public.payment set status= @status WHERE order_id= @order_id";
                cmd2.Parameters.AddWithValue("order_id", model.Id);
                cmd2.Parameters.AddWithValue("status", CoreConstant.PaymentStatus.REFUNDED);
                cmd2.Prepare();
                await cmd2.ExecuteNonQueryAsync();

                await transaction.CommitAsync();
            }

            return true;
        }
        catch (System.Exception ex)
        {
            if (transaction != null) transaction.Rollback();
            Console.WriteLine($"Ex: {ex.Message} \n {ex.StackTrace}");
            throw ex;
        }
    }
}