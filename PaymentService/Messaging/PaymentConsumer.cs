using Common;
using Confluent.Kafka;
using Newtonsoft.Json;
using PaymentService.Domain;
using PaymentService.Service;

namespace PaymentService.Messing;
public class PaymentConsumer
{
    private readonly Producer _producer;
    private readonly RetryUtil _retryUtil;
    private readonly IAccountBalanceService _accountBalanceService;
    private readonly IEventService _eventService;
    private readonly IPaymentService _paymentService;
    protected readonly ConsumerConfig consumerConfig;
    protected readonly IConsumer<Ignore, string> consumer;
    public PaymentConsumer(Producer producer, RetryUtil retryUtil, IAccountBalanceService accountBalanceService, IEventService eventService, IPaymentService paymentService)
    {
        _producer = producer;
        _retryUtil = retryUtil;
        _accountBalanceService = accountBalanceService;
        _eventService = eventService;
        _paymentService = paymentService;
        consumerConfig = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            GroupId = "PAYMENT_GROUP",
            AutoOffsetReset = AutoOffsetReset.Latest,
        };

        consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        consumer.Subscribe(new string[] { "TICKET_CREATE_PENDING", "SHIPPER_NOT_FOUND" });
    }

    public async void ReadMessage()
    {

        while (true)
        {
            try
            {
                var consumeResult = consumer.Consume();
                var topic = consumeResult.Topic;
                switch (topic)
                {
                    case CoreConstant.Topic.TICKET_CREATE_PENDING:
                        {
                            var message = consumeResult.Message.Value;

                            var createOrderMessage = JsonConvert.DeserializeObject<CreateOrderMessage>(message);
                            // kiem tra tai khoan
                            var accountBalance = await _accountBalanceService.GetByAccount(createOrderMessage.OrderBy);
                            if (createOrderMessage.TotalMoney > accountBalance.Balance)
                            {
                                // tai khoan khong du
                                await _eventService.CreateEvent(new Event() { Type = CoreConstant.Topic.PAYMENT_BALANCE_NOT_ENOUGH, Data = message, Status = CoreConstant.EventStatus.NEW });
                            }
                            else
                            {
                                try
                                {
                                    // tai khoan đủ => trừ tiền
                                    await _accountBalanceService.UpdateBalance(createOrderMessage, (accountBalance.Balance - createOrderMessage.TotalMoney));
                                }
                                catch (System.Exception ex)
                                {
                                    await _eventService.CreateEvent(new Event() { Type = CoreConstant.Topic.PAYMENT_FAILED, Data = message, Status = CoreConstant.EventStatus.NEW });
                                }
                            }

                            break;
                        }
                    case CoreConstant.Topic.SHIPPER_NOT_FOUND:
                        {
                            var message = consumeResult.Message.Value;
                            var createOrderMessage = JsonConvert.DeserializeObject<CreateOrderMessage>(message);

                            var payment = await _paymentService.GetByOrderId(createOrderMessage.Id);
                            if (payment.Status == CoreConstant.PaymentStatus.PAID)
                            {
                                // hoàn tiền
                                var accountBalance = await _accountBalanceService.GetByAccount(createOrderMessage.OrderBy);
                                await _accountBalanceService.RevertBalance(createOrderMessage, accountBalance.Balance + createOrderMessage.TotalMoney);
                            }
                            break;
                        }
                    default:
                        break;
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

}