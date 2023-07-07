using Confluent.Kafka;

namespace OrderService.Messing;
public class Producer
{
    protected readonly ProducerConfig producerConfig;
    public Producer()
    {
        producerConfig = new ProducerConfig
        {
            BootstrapServers = "localhost:9092",
        };
    }

    public async Task Publish(string topic, string message)
    {

        using (var p = new ProducerBuilder<Null, string>(producerConfig).Build())
        {
            var result = await p.ProduceAsync(topic, new Message<Null, string> { Value = message });

            // wait for up to 10 seconds for any inflight messages to be delivered.
            p.Flush(TimeSpan.FromSeconds(10));
        }
    }
}