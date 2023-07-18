using System.Reflection;
using Common;
using Microsoft.AspNetCore;
using Microsoft.OpenApi.Models;
using OrderService.Messing;
using OrderService.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSingleton<Producer>();
builder.Services.AddSingleton<RetryUtil>();
builder.Services.AddSingleton<IOrderService, OrderService.Service.OrderService>();
builder.Services.AddSingleton<IEventService, OrderService.Service.EventService>();
builder.Services.AddSingleton<IOrderHistoryService, OrderService.Service.OrderHistoryService>();
builder.Services.AddHostedService<OrderConsumer>();
builder.Services.AddHostedService<OrderProducer>();

// builder.Logging.AddConsole();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Order API",
        Description = "Order PIZZA and payment online",
        // TermsOfService = new Uri("https://example.com/terms"),
        // Contact = new OpenApiContact
        // {
        //     Name = "Example Contact",
        //     Url = new Uri("https://example.com/contact")
        // },
        // License = new OpenApiLicense
        // {
        //     Name = "Example License",
        //     Url = new Uri("https://example.com/license")
        // }
    });
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

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
