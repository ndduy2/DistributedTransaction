using Confluent.Kafka;
using Newtonsoft.Json;
using DeliveryService.Domain;
using DeliveryService.Service;
using Microsoft.Extensions.Hosting;
using Common;

namespace DeliveryService.Messing;
public class DeliveryProducer : BackgroundService
{
    private readonly Producer _producer;
    private readonly IEventService _eventService;
    public DeliveryProducer(Producer producer, IEventService eventService)
    {
        _producer = producer;
        _eventService = eventService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine($"DeliveryProducer");
        await Task.Run(async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var ev = await _eventService.GetNextPendingEvent();
                    if (ev != null)
                    {
                        var status = await _producer.Publish(ev.Type, ev.Data);
                        if (status)
                        {
                            await _eventService.UpdateEventStatus(ev.Id, CoreConstant.EventStatus.PUBLISHED);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine($"Failed: {ex.Message}\n{ex.StackTrace}");
                }
                finally
                {
                    await Task.Delay(2000, stoppingToken);
                }
            }
        });
    }
}