using Confluent.Kafka;
using Newtonsoft.Json;

namespace DeliveryService.Messing;
public class DeliveryConsumer
{
    private readonly Producer _producer;
    protected readonly ConsumerConfig consumerConfig;
    protected readonly IConsumer<Ignore, string> consumer;
    public DeliveryConsumer(Producer producer)
    {
        _producer = producer;
        consumerConfig = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            GroupId = "DELIVERY_GROUP",
            AutoOffsetReset = AutoOffsetReset.Latest,
        };

        consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        consumer.Subscribe(new string[] { "RESTAURANT_DONE" });
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
                    case "RESTAURANT_DONE":
                        {
                            var message = consumeResult.Message.Value;
                            var createOrderMessage = JsonConvert.DeserializeObject<CreateOrderMessage>(message);
                            //bussiness logic
                            if (createOrderMessage.Amount % 100000 == 0)
                            {
                                await _producer.Publish("DELIVERY_DONE", message);
                            }
                            else
                            {
                                await _producer.Publish("DELIVERY_FAILED", message);
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