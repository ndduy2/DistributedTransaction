using Confluent.Kafka;
using Newtonsoft.Json;
using PaymentService.Service;

namespace PaymentService.Messing;
public class PaymentConsumer
{
    private readonly Producer _producer;
    private readonly IAccountBalanceService _accountBalanceService;
    protected readonly ConsumerConfig consumerConfig;
    protected readonly IConsumer<Ignore, string> consumer;
    public PaymentConsumer(Producer producer, IAccountBalanceService accountBalanceService)
    {
        _producer = producer;
        _accountBalanceService = accountBalanceService;
        consumerConfig = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            GroupId = "PAYMENT_GROUP",
            AutoOffsetReset = AutoOffsetReset.Latest,
        };

        consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        consumer.Subscribe(new string[] { "ORDER_CREATED", "RESTAURANT_FAILED", "DELIVERY_FAILED" });
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
                    case "ORDER_CREATED":
                        {
                            var message = consumeResult.Message.Value;
                            var createOrderMessage = JsonConvert.DeserializeObject<CreateOrderMessage>(message);
                            // kiem tra tai khoan
                            var accountBalance = await _accountBalanceService.GetByAccount(createOrderMessage.OrderBy);
                            if (createOrderMessage.Amount > accountBalance.Balance)
                            {
                                // tai khoan khong du raise event PAYMENT_FAILED
                                await _producer.Publish("PAYMENT_FAILED", message);
                            }
                            else
                            {
                                // tai khoan du UPDATE va raise event PAYMENT_SUCCESSED
                                await _accountBalanceService.UpdateBalance(createOrderMessage.OrderBy, (accountBalance.Balance - createOrderMessage.Amount));

                                await _producer.Publish("PAYMENT_SUCCESSED", message);
                            }
                            break;
                        }
                    case "RESTAURANT_FAILED":
                        {
                            var message = consumeResult.Message.Value;
                            var createOrderMessage = JsonConvert.DeserializeObject<CreateOrderMessage>(message);
                            var accountBalance = await _accountBalanceService.GetByAccount(createOrderMessage.OrderBy);
                            await _accountBalanceService.UpdateBalance(createOrderMessage.OrderBy, (accountBalance.Balance + createOrderMessage.Amount));
                            break;
                        }
                    case "DELIVERY_FAILED":
                        {
                            var message = consumeResult.Message.Value;
                            var createOrderMessage = JsonConvert.DeserializeObject<CreateOrderMessage>(message);
                            var accountBalance = await _accountBalanceService.GetByAccount(createOrderMessage.OrderBy);
                            await _accountBalanceService.UpdateBalance(createOrderMessage.OrderBy, (accountBalance.Balance + createOrderMessage.Amount));
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