namespace PaymentService.Domain;
public class PaymentLog
{
    public int OrderId { get; set; }
    public bool Paid { get; set; }
}