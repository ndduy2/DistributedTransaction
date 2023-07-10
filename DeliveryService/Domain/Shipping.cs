namespace DeliveryService.Domain;
public class Shipping
{
    public int OrderId { get; set; }
    public string Shipper { get; set; }
    public string Customer { get; set; }
    public string RestaurantStatus { get; set; }
}