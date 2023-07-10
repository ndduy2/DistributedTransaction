using PaymentService.Domain;

namespace PaymentService.Service;
public interface IPaymentLogService
{
    Task<bool> Create(PaymentLog model);
    Task<bool> UpdateStatus(int orderId, bool paid);
    Task<PaymentLog> GetByOrderId(int orderId);
}