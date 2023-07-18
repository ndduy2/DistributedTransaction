using PaymentService.Domain;
using PaymentService.Messing;

namespace PaymentService.Service;
public interface IAccountBalanceService
{
    Task<AccountBalance> GetByAccount(string account);
    Task<bool> UpdateBalance(CreateOrderMessage model, int balance);
    Task<bool> RevertBalance(CreateOrderMessage model, int balance);
}