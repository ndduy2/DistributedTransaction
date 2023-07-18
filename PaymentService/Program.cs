using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PaymentService.Messing;
using PaymentService.Service;

namespace PaymentService;
class Program
{
    static void Main(string[] args)
    {

        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

        IConfiguration configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appSettings.json", false)
               .Build();

        builder.Services.AddSingleton<IConfiguration>(configuration);
        builder.Services.AddSingleton<Producer>();
        builder.Services.AddSingleton<RetryUtil>();
        builder.Services.AddSingleton<IAccountBalanceService, PaymentService.Service.AccountBalanceService>();
        builder.Services.AddSingleton<IEventService, EventService>();
        builder.Services.AddSingleton<IPaymentService, PaymentService.Service.PaymentService>();
        builder.Services.AddHostedService<PaymentProducer>();

        builder.Services.AddSingleton<PaymentConsumer>();

        var app = builder.Build();
        PaymentConsumer consumer = app.Services.GetService<PaymentConsumer>();
        consumer.ReadMessage();

        app.Run();
        Console.ReadLine();
    }
}