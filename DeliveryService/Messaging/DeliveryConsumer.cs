using Common;
using Confluent.Kafka;
using DeliveryService.Domain;
using DeliveryService.Service;
using Newtonsoft.Json;

namespace DeliveryService.Messing;
public class DeliveryConsumer
{
    private readonly Producer _producer;
    private readonly RetryUtil _retryUtil;
    private readonly IShipInfoService _shippingService;
    private readonly IEventService _eventService;
    protected readonly ConsumerConfig consumerConfig;
    protected readonly IConsumer<Ignore, string> consumer;
    public DeliveryConsumer(Producer producer, RetryUtil retryUtil, IShipInfoService shippingService, IEventService eventService)
    {
        _producer = producer;
        _retryUtil = retryUtil;
        _shippingService = shippingService;
        _eventService = eventService;
        consumerConfig = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            GroupId = "DELIVERY_GROUP",
            AutoOffsetReset = AutoOffsetReset.Latest,
        };

        consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        consumer.Subscribe(new string[] { "PAYMENT_SUCCESS" });
    }

    public async void ReadMessage()
    {
        try
        {
            while (true)
            {
                var consumeResult = consumer.Consume();
                var topic = consumeResult.Topic;
                switch (topic)
                {
                    case CoreConstant.Topic.PAYMENT_SUCCESS:
                        {
                            var message = consumeResult.Message.Value;
                            try
                            {
                                var createOrderMessage = JsonConvert.DeserializeObject<CreateOrderMessage>(message);
                                await _retryUtil.Retry(() => _shippingService.FindShipper(createOrderMessage), 5, 2000);
                            }
                            catch (System.Exception ex)
                            {
                                await _eventService.CreateEvent(new Event() { Type = CoreConstant.Topic.SHIPPER_NOT_FOUND, Data = message, Status = CoreConstant.EventStatus.NEW });
                            }

                            break;
                        }
                    default:
                        break;
                }
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error consuming messages from Kafka - Reason:{ex}");
        }
        finally
        {
        }
    }

}