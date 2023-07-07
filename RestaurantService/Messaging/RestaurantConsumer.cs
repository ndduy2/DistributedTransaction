using Confluent.Kafka;
using Newtonsoft.Json;

namespace RestaurantService.Messing;
public class RestaurantConsumer
{
    private readonly Producer _producer;
    protected readonly ConsumerConfig consumerConfig;
    protected readonly IConsumer<Ignore, string> consumer;
    public RestaurantConsumer(Producer producer)
    {
        _producer = producer;
        consumerConfig = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            GroupId = "RESTAURANT_GROUP",
            AutoOffsetReset = AutoOffsetReset.Latest,
        };

        consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        consumer.Subscribe(new string[] { "PAYMENT_SUCCESSED" });
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
                            //bussiness logic
                            if (createOrderMessage.Amount % 2 == 0)
                            {
                                await _producer.Publish("RESTAURANT_DONE", message);
                            }
                            else
                            {
                                await _producer.Publish("RESTAURANT_FAILED", message);
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