using PaymentService.Domain;

namespace PaymentService.Service;
public interface IPaymentService
{
    Task<Payment> GetByOrderId(int orderId);
}