using Common;
using Confluent.Kafka;
using Newtonsoft.Json;
using OrderService.Domain;
using OrderService.Service;

namespace OrderService.Messing;
public class OrderConsumer : BackgroundService
{
    private readonly Producer _producer;
    private readonly RetryUtil _retryUtil;
    private readonly IOrderService _orderService;
    private readonly IOrderHistoryService _orderHistoryService;
    protected readonly ConsumerConfig consumerConfig;
    protected readonly IConsumer<Ignore, string> consumer;
    public OrderConsumer(Producer producer, RetryUtil retryUtil, IOrderService orderService, IOrderHistoryService orderHistoryService)
    {
        _producer = producer;
        _retryUtil = retryUtil;
        _orderService = orderService;
        _orderHistoryService = orderHistoryService;
        consumerConfig = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            GroupId = "ORDER_GROUP",
            AutoOffsetReset = AutoOffsetReset.Latest,
        };

        consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        consumer.Subscribe(new string[] { "RESTAURANT_RETRYLATER", "RESTAURANT_ERROR", "RESTAURANT_STOCK_NOT_ENOUGH", "PAYMENT_BALANCE_NOT_ENOUGH",
        "PAYMENT_FAILED", "PAYMENT_SUCCESS", "SHIPPER_FOUND", "TICKET_DONE", "SHIPPER_NOT_FOUND"});
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        try
        {
            await Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var consumeResult = consumer.Consume();
                    var topic = consumeResult.Topic;
                    switch (topic)
                    {
                        case CoreConstant.Topic.RESTAURANT_RETRYLATER:
                            {
                                var message = consumeResult.Message.Value;
                                var createOrderMessage = JsonConvert.DeserializeObject<CreateOrderMessage>(message);
                                var order = await _orderService.GetOneById(createOrderMessage.Id);
                                if (order.Status == CoreConstant.OrderStatus.CREATE_PENDING)
                                {
                                    await _orderService.UpdateOrder(createOrderMessage.Id, CoreConstant.OrderStatus.CANCEL_RETRYLATER);
                                    await _orderHistoryService.Create(new OrderHistory() { OrderId = createOrderMessage.Id, Status = CoreConstant.OrderStatus.CANCEL_RETRYLATER, Ts = DateTime.Now });
                                }
                                break;
                            }
                        case CoreConstant.Topic.RESTAURANT_ERROR:
                            {
                                var message = consumeResult.Message.Value;
                                var createOrderMessage = JsonConvert.DeserializeObject<CreateOrderMessage>(message);
                                var order = await _orderService.GetOneById(createOrderMessage.Id);
                                if (order.Status == CoreConstant.OrderStatus.CREATE_PENDING)
                                {
                                    await _orderService.UpdateOrder(createOrderMessage.Id, CoreConstant.OrderStatus.CANCEL_RESTAURANT_ERROR);
                                    await _orderHistoryService.Create(new OrderHistory() { OrderId = createOrderMessage.Id, Status = CoreConstant.OrderStatus.CANCEL_RESTAURANT_ERROR, Ts = DateTime.Now });
                                }
                                break;
                            }
                        case CoreConstant.Topic.RESTAURANT_STOCK_NOT_ENOUGH:
                            {
                                var message = consumeResult.Message.Value;
                                var createOrderMessage = JsonConvert.DeserializeObject<CreateOrderMessage>(message);
                                var order = await _orderService.GetOneById(createOrderMessage.Id);
                                if (order.Status == CoreConstant.OrderStatus.CREATE_PENDING)
                                {
                                    await _orderService.UpdateOrder(createOrderMessage.Id, CoreConstant.OrderStatus.CANCEL_STOCK_NOT_ENOUGH);
                                    await _orderHistoryService.Create(new OrderHistory() { OrderId = createOrderMessage.Id, Status = CoreConstant.OrderStatus.CANCEL_STOCK_NOT_ENOUGH, Ts = DateTime.Now });
                                }
                                break;
                            }
                        case CoreConstant.Topic.PAYMENT_BALANCE_NOT_ENOUGH:
                            {
                                var message = consumeResult.Message.Value;
                                var createOrderMessage = JsonConvert.DeserializeObject<CreateOrderMessage>(message);
                                var order = await _orderService.GetOneById(createOrderMessage.Id);
                                if (order.Status == CoreConstant.OrderStatus.CREATE_PENDING)
                                {
                                    await _orderService.UpdateOrder(createOrderMessage.Id, CoreConstant.OrderStatus.CANCEL_BALANCE_NOT_ENOUGH);
                                    await _orderHistoryService.Create(new OrderHistory() { OrderId = createOrderMessage.Id, Status = CoreConstant.OrderStatus.CANCEL_BALANCE_NOT_ENOUGH, Ts = DateTime.Now });
                                }
                                break;
                            }
                        case CoreConstant.Topic.PAYMENT_FAILED:
                            {
                                var message = consumeResult.Message.Value;
                                var createOrderMessage = JsonConvert.DeserializeObject<CreateOrderMessage>(message);
                                var order = await _orderService.GetOneById(createOrderMessage.Id);
                                if (order.Status == CoreConstant.OrderStatus.CREATE_PENDING)
                                {
                                    await _orderService.UpdateOrder(createOrderMessage.Id, CoreConstant.OrderStatus.CANCEL_PAYMENT_FAILED);
                                    await _orderHistoryService.Create(new OrderHistory() { OrderId = createOrderMessage.Id, Status = CoreConstant.OrderStatus.CANCEL_PAYMENT_FAILED, Ts = DateTime.Now });
                                }
                                break;
                            }
                        case CoreConstant.Topic.PAYMENT_SUCCESS:
                            {
                                var message = consumeResult.Message.Value;
                                var createOrderMessage = JsonConvert.DeserializeObject<CreateOrderMessage>(message);
                                var order = await _orderService.GetOneById(createOrderMessage.Id);
                                if (order.Status == CoreConstant.OrderStatus.CREATE_PENDING)
                                {
                                    await _retryUtil.Retry<int>(() => _orderService.UpdateOrder(createOrderMessage.Id, CoreConstant.OrderStatus.PAID));
                                    await _orderHistoryService.Create(new OrderHistory() { OrderId = createOrderMessage.Id, Status = CoreConstant.OrderStatus.PAID, Ts = DateTime.Now });
                                }
                                break;
                            }
                        case CoreConstant.Topic.SHIPPER_FOUND:
                            {
                                var message = consumeResult.Message.Value;
                                var createOrderMessage = JsonConvert.DeserializeObject<CreateOrderMessage>(message);
                                var order = await _orderService.GetOneById(createOrderMessage.Id);
                                if (order.Status == CoreConstant.OrderStatus.PAID || order.Status == CoreConstant.OrderStatus.READYTODELIVER)
                                {
                                    await _retryUtil.Retry<int>(() => _orderService.UpdateOrderShipper(createOrderMessage.Id, createOrderMessage.Shipper));
                                    await _orderHistoryService.Create(new OrderHistory() { OrderId = createOrderMessage.Id, Status = CoreConstant.Topic.SHIPPER_FOUND, Ts = DateTime.Now });
                                }

                                await Task.Delay(5000);
                                order = await _orderService.GetOneById(createOrderMessage.Id);
                                if (!string.IsNullOrWhiteSpace(order.Shipper) && order.Status == CoreConstant.OrderStatus.READYTODELIVER)
                                {
                                    await _retryUtil.Retry<int>(() => _orderService.UpdateOrder(createOrderMessage.Id, CoreConstant.OrderStatus.COMPLETED));
                                    await _orderHistoryService.Create(new OrderHistory() { OrderId = createOrderMessage.Id, Status = CoreConstant.OrderStatus.COMPLETED, Ts = DateTime.Now });
                                }
                                break;
                            }
                        case CoreConstant.Topic.SHIPPER_NOT_FOUND:
                            {
                                var message = consumeResult.Message.Value;
                                var createOrderMessage = JsonConvert.DeserializeObject<CreateOrderMessage>(message);
                                var order = await _orderService.GetOneById(createOrderMessage.Id);
                                if (order.Status == CoreConstant.OrderStatus.PAID || order.Status == CoreConstant.OrderStatus.READYTODELIVER)
                                {
                                    await _retryUtil.Retry<int>(() => _orderService.UpdateOrder(createOrderMessage.Id, CoreConstant.OrderStatus.CANCEL_SHIPPER_NOT_FOUND));
                                    await _orderHistoryService.Create(new OrderHistory() { OrderId = createOrderMessage.Id, Status = CoreConstant.OrderStatus.CANCEL_SHIPPER_NOT_FOUND, Ts = DateTime.Now });
                                }
                                break;
                            }
                        case CoreConstant.Topic.TICKET_DONE:
                            {
                                var message = consumeResult.Message.Value;
                                var createOrderMessage = JsonConvert.DeserializeObject<CreateOrderMessage>(message);
                                var order = await _orderService.GetOneById(createOrderMessage.Id);
                                if (order.Status == CoreConstant.OrderStatus.PAID)
                                {
                                    await _retryUtil.Retry<int>(() => _orderService.UpdateOrder(createOrderMessage.Id, CoreConstant.OrderStatus.READYTODELIVER));
                                    await _orderHistoryService.Create(new OrderHistory() { OrderId = createOrderMessage.Id, Status = CoreConstant.OrderStatus.READYTODELIVER, Ts = DateTime.Now });
                                }

                                await Task.Delay(5000);
                                order = await _orderService.GetOneById(createOrderMessage.Id);
                                if (!string.IsNullOrWhiteSpace(order.Shipper) && order.Status == CoreConstant.OrderStatus.READYTODELIVER)
                                {
                                    await _retryUtil.Retry<int>(() => _orderService.UpdateOrder(createOrderMessage.Id, CoreConstant.OrderStatus.COMPLETED));
                                    await _orderHistoryService.Create(new OrderHistory() { OrderId = createOrderMessage.Id, Status = CoreConstant.OrderStatus.COMPLETED, Ts = DateTime.Now });
                                }

                                break;
                            }
                        default:
                            break;
                    }
                }
            });

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error consuming messages from Kafka - Reason:{ex}");
        }
    }

}