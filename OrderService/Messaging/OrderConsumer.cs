using Confluent.Kafka;
using Newtonsoft.Json;
using OrderService.Service;

namespace OrderService.Messing;
public class OrderConsumer : BackgroundService
{
    private readonly Producer _producer;
    private readonly IOrderService _orderService;
    protected readonly ConsumerConfig consumerConfig;
    protected readonly IConsumer<Ignore, string> consumer;
    public OrderConsumer(Producer producer, IOrderService orderService)
    {
        _producer = producer;
        _orderService = orderService;
        consumerConfig = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            GroupId = "ORDER_GROUP",
            AutoOffsetReset = AutoOffsetReset.Latest,
        };

        consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        consumer.Subscribe(new string[] { "PAYMENT_SUCCESSED", "PAYMENT_FAILED", "RESTAURANT_DONE", "RESTAURANT_FAILED", "DELIVERY_DONE", "DELIVERY_FAILED" });
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
                        case "PAYMENT_SUCCESSED":
                            {
                                var message = consumeResult.Message.Value;
                                var createOrderMessage = JsonConvert.DeserializeObject<CreateOrderMessage>(message);
                                await _orderService.UpdateOrder(createOrderMessage.Id, "PAID");
                                break;
                            }
                        case "PAYMENT_FAILED":
                            {
                                var message = consumeResult.Message.Value;
                                var createOrderMessage = JsonConvert.DeserializeObject<CreateOrderMessage>(message);
                                await _orderService.UpdateOrder(createOrderMessage.Id, "CANCELED_PAYMENT");
                                break;
                            }
                        case "RESTAURANT_DONE":
                            {
                                var message = consumeResult.Message.Value;
                                var createOrderMessage = JsonConvert.DeserializeObject<CreateOrderMessage>(message);
                                await _orderService.UpdateOrder(createOrderMessage.Id, "PENDDING_DELIVER");
                                break;
                            }
                        case "RESTAURANT_FAILED":
                            {
                                var message = consumeResult.Message.Value;
                                var createOrderMessage = JsonConvert.DeserializeObject<CreateOrderMessage>(message);
                                await _orderService.UpdateOrder(createOrderMessage.Id, "CANCELED_RESTAURANT");
                                break;
                            }
                        case "DELIVERY_DONE":
                            {
                                var message = consumeResult.Message.Value;
                                var createOrderMessage = JsonConvert.DeserializeObject<CreateOrderMessage>(message);
                                await _orderService.UpdateOrder(createOrderMessage.Id, "COMPLETED");
                                break;
                            }
                        case "DELIVERY_FAILED":
                            {
                                var message = consumeResult.Message.Value;
                                var createOrderMessage = JsonConvert.DeserializeObject<CreateOrderMessage>(message);
                                await _orderService.UpdateOrder(createOrderMessage.Id, "CANCELED_DELIVERY");
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