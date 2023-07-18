namespace OrderService.Domain;
public class Order
{
    public int Id { get; set; }
    public string OrderBy { get; set; }
    public string Product { get; set; }
    public int Quantity { get; set; }
    public int TotalMoney { get; set; }
    public string Status { get; set; }
    public string Shipper { get; set; }
}