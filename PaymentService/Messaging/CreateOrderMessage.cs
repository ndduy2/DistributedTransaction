namespace PaymentService.Messing;
public class CreateOrderMessage
{
    public int Id { get; set; }
    public string OrderBy { get; set; }
    public int Amount { get; set; }
}