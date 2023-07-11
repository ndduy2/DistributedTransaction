using Confluent.Kafka;
using DeliveryService.Domain;
using DeliveryService.Service;
using DeliveryService.Util;
using Newtonsoft.Json;

namespace DeliveryService.Messing;
public class DeliveryConsumer
{
    private readonly Producer _producer;
    private readonly RetryUtil _retryUtil;
    private readonly IShippingService _shippingService;
    protected readonly ConsumerConfig consumerConfig;
    protected readonly IConsumer<Ignore, string> consumer;
    public DeliveryConsumer(Producer producer, RetryUtil retryUtil, IShippingService shippingService)
    {
        _producer = producer;
        _shippingService = shippingService;
        _retryUtil = retryUtil;
        consumerConfig = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            GroupId = "DELIVERY_GROUP",
            AutoOffsetReset = AutoOffsetReset.Latest,
        };

        consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        consumer.Subscribe(new string[] { "PAYMENT_SUCCESSED", "RESTAURANT_FAILED", "RESTAURANT_DONE" });
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
                            var shipping = await _shippingService.GetByOrderId(createOrderMessage.Id);
                            if (shipping == null)
                            {
                                shipping = new Shipping()
                                {
                                    OrderId = createOrderMessage.Id,
                                    Customer = createOrderMessage.OrderBy,
                                    Shipper = string.Empty,
                                    RestaurantStatus = string.Empty
                                };

                                await _shippingService.CreateShipping(shipping);
                            }

                            var shipper = string.Empty;
                            try
                            {
                                shipper = await _retryUtil.Retry<string>(
                                    () => _shippingService.FindShipper(createOrderMessage.TotalMoney), 3, 500);
                            }
                            catch (System.Exception ex)
                            {
                                await _producer.Publish("DELIVERY_FAILED", message);
                            }

                            if (!string.IsNullOrWhiteSpace(shipper))
                            {
                                await _shippingService.UpdateShipper(createOrderMessage.Id, shipper);
                            }

                            shipping = await _shippingService.GetByOrderId(createOrderMessage.Id);
                            if (shipping != null && !string.IsNullOrWhiteSpace(shipping.Shipper) && !string.IsNullOrWhiteSpace(shipping.RestaurantStatus))
                            {
                                await _producer.Publish("DELIVERY_DONE", message);
                            }
                            break;
                        }
                    case "RESTAURANT_FAILED":
                        {
                            var message = consumeResult.Message.Value;
                            var createOrderMessage = JsonConvert.DeserializeObject<CreateOrderMessage>(message);

                            var shipping = await _shippingService.GetByOrderId(createOrderMessage.Id);
                            if (shipping != null)
                            {
                                await _shippingService.UpdateShipper(createOrderMessage.Id, string.Empty);
                            }
                            break;
                        }
                    case "RESTAURANT_DONE":
                        {
                            var message = consumeResult.Message.Value;
                            var createOrderMessage = JsonConvert.DeserializeObject<CreateOrderMessage>(message);

                            var shipping = await _shippingService.GetByOrderId(createOrderMessage.Id);
                            if (shipping == null)
                            {
                                shipping = new Shipping()
                                {
                                    OrderId = createOrderMessage.Id,
                                    Customer = createOrderMessage.OrderBy,
                                    Shipper = string.Empty,
                                    RestaurantStatus = string.Empty
                                };

                                await _shippingService.CreateShipping(shipping);
                            }

                            await _shippingService.UpdateRestaurantStatus(createOrderMessage.Id, "FOOD_DONE");

                            shipping = await _shippingService.GetByOrderId(createOrderMessage.Id);
                            if (shipping != null && !string.IsNullOrWhiteSpace(shipping.Shipper) && !string.IsNullOrWhiteSpace(shipping.RestaurantStatus))
                            {
                                await _producer.Publish("DELIVERY_DONE", message);
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