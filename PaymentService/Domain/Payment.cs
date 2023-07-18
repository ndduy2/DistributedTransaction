namespace PaymentService.Domain;
public class Payment
{
    public int OrderId { get; set; }
    public string Account { get; set; }
    public int Amount { get; set; }
    public string Status { get; set; }
}