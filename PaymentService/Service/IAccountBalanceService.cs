using PaymentService.Domain;

namespace PaymentService.Service;
public interface IAccountBalanceService
{
    Task<AccountBalance> GetByAccount(string account);
    Task<bool> UpdateBalance(string account, int balance, int? orderId = null);
}