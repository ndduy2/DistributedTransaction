using Common;
using Confluent.Kafka;
using Newtonsoft.Json;
using RestaurantService.Domain;
using RestaurantService.Service;

namespace RestaurantService.Messing;
public class RestaurantConsumer
{
    private readonly Producer _producer;
    private readonly RetryUtil _retryUtil;
    private readonly IInventoryService _inventoryService;
    private readonly IEventService _eventService;
    private readonly ITicketService _ticketService;
    protected readonly ConsumerConfig consumerConfig;
    protected readonly IConsumer<Ignore, string> consumer;
    public RestaurantConsumer(Producer producer, RetryUtil retryUtil, IInventoryService inventoryService, IEventService eventService, ITicketService ticketService)
    {
        _producer = producer;
        _retryUtil = retryUtil;
        _inventoryService = inventoryService;
        _eventService = eventService;
        _ticketService = ticketService;
        consumerConfig = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            GroupId = "RESTAURANT_GROUP",
            AutoOffsetReset = AutoOffsetReset.Latest,
        };

        consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        consumer.Subscribe(new string[] { "ORDER_CREATE_PENDING", "PAYMENT_BALANCE_NOT_ENOUGH", "PAYMENT_FAILED", "PAYMENT_SUCCESS", "TICKET_APPROVED", "SHIPPER_NOT_FOUND" });
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
                    case CoreConstant.Topic.ORDER_CREATE_PENDING:
                        {
                            var message = consumeResult.Message.Value;
                            var createOrderMessage = JsonConvert.DeserializeObject<CreateOrderMessage>(message);

                            var inventory = await _inventoryService.GetAmount(createOrderMessage.Product);
                            // bản ghi đang bị lock thì hủy đơn hàng
                            if (inventory?.Status != CoreConstant.InventoryStatus.AVAIABLE)
                            {
                                await _eventService.CreateEvent(new Event() { Type = CoreConstant.Topic.RESTAURANT_RETRYLATER, Data = message, Status = CoreConstant.EventStatus.NEW });
                            }
                            else
                            {
                                if (createOrderMessage.Quantity <= inventory.Stock)
                                {
                                    try
                                    {
                                        await _inventoryService.UpdateStock(createOrderMessage, inventory.Stock - createOrderMessage.Quantity);
                                    }
                                    catch (System.Exception ex)
                                    {
                                        await _eventService.CreateEvent(new Event() { Type = CoreConstant.Topic.RESTAURANT_ERROR, Data = message, Status = CoreConstant.EventStatus.NEW });
                                    }
                                }
                                else
                                {
                                    await _eventService.CreateEvent(new Event() { Type = CoreConstant.Topic.RESTAURANT_STOCK_NOT_ENOUGH, Data = message, Status = CoreConstant.EventStatus.NEW });
                                }
                            }

                            break;
                        }
                    case CoreConstant.Topic.PAYMENT_BALANCE_NOT_ENOUGH:
                        {
                            var message = consumeResult.Message.Value;
                            var createOrderMessage = JsonConvert.DeserializeObject<CreateOrderMessage>(message);
                            var ticket = await _ticketService.GetByOrderId(createOrderMessage.Id);
                            if (ticket.Status == CoreConstant.TicketStatus.CREATE_PENDING)
                            {
                                //Hoàn hàng trong kho
                                var inventory = await _inventoryService.GetAmount(createOrderMessage.Product);
                                await _inventoryService.RevertStock(createOrderMessage.Product, inventory.Stock + createOrderMessage.Quantity);

                                await _ticketService.UpdateStatus(createOrderMessage.Id, CoreConstant.TicketStatus.CANCELED);
                            }
                            break;
                        }
                    case CoreConstant.Topic.PAYMENT_FAILED:
                        {
                            var message = consumeResult.Message.Value;
                            var createOrderMessage = JsonConvert.DeserializeObject<CreateOrderMessage>(message);
                            var ticket = await _ticketService.GetByOrderId(createOrderMessage.Id);
                            if (ticket.Status == CoreConstant.TicketStatus.CREATE_PENDING)
                            {
                                //Hoàn hàng trong kho
                                var inventory = await _inventoryService.GetAmount(createOrderMessage.Product);
                                await _inventoryService.RevertStock(createOrderMessage.Product, inventory.Stock + createOrderMessage.Quantity);

                                await _ticketService.UpdateStatus(createOrderMessage.Id, CoreConstant.TicketStatus.CANCELED);
                            }
                            break;
                        }
                    case CoreConstant.Topic.PAYMENT_SUCCESS:
                        {
                            var message = consumeResult.Message.Value;
                            var createOrderMessage = JsonConvert.DeserializeObject<CreateOrderMessage>(message);
                            var ticket = await _ticketService.GetByOrderId(createOrderMessage.Id);
                            if (ticket.Status == CoreConstant.TicketStatus.CREATE_PENDING)
                            {
                                await _retryUtil.Retry(() => _ticketService.Approve(createOrderMessage));
                            }

                            break;
                        }
                    case CoreConstant.Topic.TICKET_APPROVED:
                        {
                            var message = consumeResult.Message.Value;
                            var createOrderMessage = JsonConvert.DeserializeObject<CreateOrderMessage>(message);
                            var ticket = await _ticketService.GetByOrderId(createOrderMessage.Id);
                            if (ticket.Status == CoreConstant.TicketStatus.APPROVED)
                            {
                                await Task.Delay(3000);
                                await _retryUtil.Retry(() => _ticketService.Done(createOrderMessage));
                            }
                            break;
                        }
                    case CoreConstant.Topic.SHIPPER_NOT_FOUND:
                        {
                            var message = consumeResult.Message.Value;
                            var createOrderMessage = JsonConvert.DeserializeObject<CreateOrderMessage>(message);
                            var ticket = await _ticketService.GetByOrderId(createOrderMessage.Id);
                            if (ticket.Status == CoreConstant.TicketStatus.APPROVED || ticket.Status == CoreConstant.TicketStatus.APPROVED)
                            {
                                //Hoàn hàng trong kho nếu chưa nấu
                                if (ticket.Status == CoreConstant.TicketStatus.APPROVED)
                                {
                                    var inventory = await _inventoryService.GetAmount(createOrderMessage.Product);
                                    await _inventoryService.RevertStock(createOrderMessage.Product, inventory.Stock + createOrderMessage.Quantity);
                                }

                                await _ticketService.UpdateStatus(createOrderMessage.Id, CoreConstant.TicketStatus.CANCELED);
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