using System.ComponentModel.DataAnnotations;

namespace OrderService.Model;

public class CreateOrderModel
{
    [Required]
    public string OrderBy { get; set; }
    [Required]
    public string Food { get; set; }
    [Required]
    public int Quantity { get; set; }
    [Required]
    public int TotalMoney { get; set; }
}