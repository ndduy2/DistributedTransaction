using Microsoft.AspNetCore;
using OrderService.Messing;
using OrderService.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSingleton<Producer>();
builder.Services.AddSingleton<IOrderService, OrderService.Service.OrderService>();
builder.Services.AddHostedService<OrderConsumer>();

// builder.Logging.AddConsole();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// public class Program
// {
//     public static void Main(string[] args)
//     {
//         CreateWebHostBuilder(args).Build().Run();
//     }

//     public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
//         WebHost.CreateDefaultBuilder(args)
//             .UseStartup<Startup>();
// }