using Common;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OrderService.Domain;
using OrderService.Messing;
using OrderService.Model;
using OrderService.Service;

namespace OrderService.Controllers;

[ApiController]
[Route("[controller]")]
public class OrderController : ControllerBase
{
    // private readonly ILogger<OrderController> _logger;
    private readonly IOrderService _orderService;
    private readonly IOrderHistoryService _orderHistoryService;
    private readonly Producer _producer;

    public OrderController(IOrderService orderService, Producer producer, IOrderHistoryService orderHistoryService)
    {
        _orderService = orderService;
        _orderHistoryService = orderHistoryService;
        _producer = producer;
    }

    /// <summary>
    /// Create new order
    /// </summary>
    /// <param name="model"></param>
    /// <returns>Id of created Order</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /Order
    ///     {
    ///        "orderBy": "DUYND",
    ///        "food": "HAISAN",
    ///        "quantity": 3,
    ///        "totalMoney": 200000,
    ///     }
    ///
    /// </remarks>
    /// <response code="201">Returns the ID newly created order</response>
    /// <response code="400">If cannot create new order</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Produces("application/json")]
    public async Task<IActionResult> Create([FromBody] CreateOrderModel model)
    {
        try
        {
            var order = new Order()
            {
                OrderBy = model.OrderBy,
                Product = model.Food,
                Quantity = model.Quantity,
                TotalMoney = model.TotalMoney,
                Shipper = string.Empty,
                Status = CoreConstant.OrderStatus.CREATE_PENDING,
            };

            var result = await _orderService.CreateOrder(order);
            if (result > 0)
            {
                await _orderHistoryService.Create(new OrderHistory() { OrderId = result, Status = CoreConstant.OrderStatus.CREATE_PENDING, Ts = DateTime.Now });
            }
            return Ok(result);
        }
        catch (System.Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}\n{ex.StackTrace}");
            return BadRequest();
        }
    }

    // [Route("UpdateOrder")]
    // [HttpPut]
    // public async Task<IActionResult> UpdateOrder([FromQuery] int orderId, [FromQuery] string status)
    // {
    //     var result = await _orderService.UpdateOrder(orderId, status);

    //     return Ok(result);
    // }

    // [HttpDelete("{id:int}")]
    // public async Task<IActionResult> DeleteOrder(int id)
    // {
    //     var result = await _orderService.DeleteOrder(id);

    //     return Ok(result);
    // }
}
