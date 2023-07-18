namespace OrderService.Domain;
public class OrderHistory
{
    public int OrderId { get; set; }
    public DateTime Ts { get; set; }
    public string Status { get; set; }
}