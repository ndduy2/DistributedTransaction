namespace OrderService.Domain;
public class Order
{
    public int Id { get; set; }
    public string OrderBy { get; set; }
    public int Amount { get; set; }
    public string Status { get; set; }
}