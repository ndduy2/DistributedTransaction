using Confluent.Kafka;
using Newtonsoft.Json;
using RestaurantService.Domain;
using RestaurantService.Service;

namespace RestaurantService.Messing;
public class RestaurantConsumer
{
    private readonly Producer _producer;
    private readonly IInventoryService _inventoryService;
    private readonly IRestaurantLogService _restaurantLogService;
    protected readonly ConsumerConfig consumerConfig;
    protected readonly IConsumer<Ignore, string> consumer;
    public RestaurantConsumer(Producer producer, IInventoryService inventoryService, IRestaurantLogService restaurantLogService)
    {
        _producer = producer;
        _inventoryService = inventoryService;
        _restaurantLogService = restaurantLogService;
        consumerConfig = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            GroupId = "RESTAURANT_GROUP",
            AutoOffsetReset = AutoOffsetReset.Latest,
        };

        consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        consumer.Subscribe(new string[] { "PAYMENT_SUCCESSED", "DELIVERY_FAILED" });
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
                    case "PAYMENT_SUCCESSED":
                        {
                            var message = consumeResult.Message.Value;
                            var createOrderMessage = JsonConvert.DeserializeObject<CreateOrderMessage>(message);

                            var inventory = await _inventoryService.GetAmount(createOrderMessage.Product);
                            if (createOrderMessage.Quantity <= inventory.Stock)
                            {
                                await _inventoryService.UpdateStock(createOrderMessage.Product, inventory.Stock - createOrderMessage.Quantity, createOrderMessage.Id);
                                await _producer.Publish("RESTAURANT_DONE", message);
                            }
                            else
                            {
                                await _producer.Publish("RESTAURANT_FAILED", message);
                            }
                            break;
                        }
                    case "DELIVERY_FAILED":
                        {
                            var message = consumeResult.Message.Value;
                            var createOrderMessage = JsonConvert.DeserializeObject<CreateOrderMessage>(message);

                            var restaurantLog = await _restaurantLogService.GetByOrderId(createOrderMessage.Id);
                            //trả lại đồ trong kho
                            if (restaurantLog != null && restaurantLog.IsCooking)
                            {
                                var inventory = await _inventoryService.GetAmount(createOrderMessage.Product);
                                await _inventoryService.UpdateStock(createOrderMessage.Product, inventory.Stock + createOrderMessage.Quantity);
                                await _restaurantLogService.UpdateStatus(createOrderMessage.Id, false);
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
            consumer.Close();
        }
    }
}