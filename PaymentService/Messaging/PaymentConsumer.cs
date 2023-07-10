using Confluent.Kafka;
using Newtonsoft.Json;
using PaymentService.Domain;
using PaymentService.Service;

namespace PaymentService.Messing;
public class PaymentConsumer
{
    private readonly Producer _producer;
    private readonly IAccountBalanceService _accountBalanceService;
    private readonly IPaymentLogService _paymentLogService;
    protected readonly ConsumerConfig consumerConfig;
    protected readonly IConsumer<Ignore, string> consumer;
    public PaymentConsumer(Producer producer, IAccountBalanceService accountBalanceService, IPaymentLogService paymentLogService)
    {
        _producer = producer;
        _accountBalanceService = accountBalanceService;
        _paymentLogService = paymentLogService;
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
                            if (createOrderMessage.TotalMoney > accountBalance.Balance)
                            {
                                // tai khoan khong du raise event PAYMENT_FAILED
                                await _producer.Publish("PAYMENT_FAILED", message);
                            }
                            else
                            {
                                // tai khoan du UPDATE va raise event PAYMENT_SUCCESSED
                                await _accountBalanceService.UpdateBalance(createOrderMessage.OrderBy, (accountBalance.Balance - createOrderMessage.TotalMoney), createOrderMessage.Id);

                                await _producer.Publish("PAYMENT_SUCCESSED", message);
                            }
                            break;
                        }
                    case "RESTAURANT_FAILED":
                        {
                            var message = consumeResult.Message.Value;
                            var createOrderMessage = JsonConvert.DeserializeObject<CreateOrderMessage>(message);
                            var paymentLog = await _paymentLogService.GetByOrderId(createOrderMessage.Id);
                            // hoàn tiền
                            if (paymentLog != null && paymentLog.Paid)
                            {
                                var accountBalance = await _accountBalanceService.GetByAccount(createOrderMessage.OrderBy);
                                await _accountBalanceService.UpdateBalance(createOrderMessage.OrderBy, (accountBalance.Balance + createOrderMessage.TotalMoney));
                                await _paymentLogService.UpdateStatus(createOrderMessage.Id, false);
                            }
                            break;
                        }
                    case "DELIVERY_FAILED":
                        {
                            var message = consumeResult.Message.Value;
                            var createOrderMessage = JsonConvert.DeserializeObject<CreateOrderMessage>(message);
                            var paymentLog = await _paymentLogService.GetByOrderId(createOrderMessage.Id);
                            // hoàn tiền
                            if (paymentLog != null && paymentLog.Paid)
                            {
                                var accountBalance = await _accountBalanceService.GetByAccount(createOrderMessage.OrderBy);
                                await _accountBalanceService.UpdateBalance(createOrderMessage.OrderBy, (accountBalance.Balance + createOrderMessage.TotalMoney));
                                await _paymentLogService.UpdateStatus(createOrderMessage.Id, false);
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