namespace OrderService.Domain;
public class Event
{
    public int Id { get; set; }
    public string Type { get; set; }
    public string Data { get; set; }
    public string Status { get; set; }
}