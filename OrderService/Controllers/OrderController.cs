using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OrderService.Domain;
using OrderService.Messing;
using OrderService.Service;

namespace OrderService.Controllers;

[ApiController]
[Route("[controller]")]
public class OrderController : ControllerBase
{
    // private readonly ILogger<OrderController> _logger;
    private readonly IOrderService _orderService;
    private readonly Producer _producer;

    public OrderController(IOrderService orderService, Producer producer)
    {
        // _logger = logger;
        _orderService = orderService;
        _producer = producer;
    }

    [HttpPost]
    public async Task<IActionResult> AddOrder([FromBody] Order order)
    {
        var result = await _orderService.CreateOrder(order);

        if (result > 0)
        {
            var newOrder = await _orderService.GetOneById(result);
            var createOrderMessage = new CreateOrderMessage()
            {
                Id = newOrder.Id,
                OrderBy = newOrder.OrderBy,
                Amount = newOrder.Amount
            };
            var message = JsonConvert.SerializeObject(createOrderMessage);
            await _producer.Publish("ORDER_CREATED", message);
        }
        return Ok(true);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateOrder([FromQuery] int orderId, [FromQuery] string status)
    {
        var result = await _orderService.UpdateOrder(orderId, status);

        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        var result = await _orderService.DeleteOrder(id);

        return Ok(result);
    }
}
