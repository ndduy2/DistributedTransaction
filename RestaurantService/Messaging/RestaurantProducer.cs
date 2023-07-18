using Common;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RestaurantService.Domain;
using RestaurantService.Service;

namespace RestaurantService.Messing;
public class RestaurantProducer : BackgroundService
{
    private readonly Producer _producer;
    private readonly IEventService _eventService;
    public RestaurantProducer(Producer producer, IEventService eventService)
    {
        _producer = producer;
        _eventService = eventService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine($"RestaurantProducer");
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