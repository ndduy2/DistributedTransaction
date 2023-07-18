using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DeliveryService.Messing;
using DeliveryService.Service;
using Common;

namespace DeliveryService;
class Program
{
    static void Main(string[] args)
    {

        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

        IConfiguration configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appSettings.json", false)
               .Build();

        builder.Services.AddSingleton<Producer>();
        builder.Services.AddSingleton<RetryUtil>();
        builder.Services.AddSingleton<IEventService, EventService>();
        builder.Services.AddSingleton<IShipInfoService, ShipInfoService>();
        builder.Services.AddHostedService<DeliveryProducer>();

        builder.Services.AddSingleton<DeliveryConsumer>();

        var app = builder.Build();
        DeliveryConsumer consumer = app.Services.GetService<DeliveryConsumer>();
        consumer.ReadMessage();

        app.Run();
        Console.ReadLine();
    }
}