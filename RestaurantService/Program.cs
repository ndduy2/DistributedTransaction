﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RestaurantService.Messing;

namespace RestaurantService;
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

        builder.Services.AddSingleton<RestaurantConsumer>();

        var app = builder.Build();
        RestaurantConsumer consumer = app.Services.GetService<RestaurantConsumer>();
        consumer.ReadMessage();
        
        app.Run();
        Console.ReadLine();
    }
}