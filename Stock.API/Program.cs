
using MassTransit;
using MongoDB.Driver;
using Shared;
using Stock.API.Consumers;
using Stock.API.Services;

namespace Stock.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddMassTransit(configurator =>
        {
            configurator.AddConsumer<OrderCreatedEventConsumer>();
            configurator.UsingRabbitMq((context, _configurator) =>
            {
                _configurator.Host("localhost", "/", conf =>
                {
                    conf.Username("guest");
                    conf.Password("guest");
                });
                _configurator.ReceiveEndpoint(RabbitMQSettings.Stock_OrderCreatedEventQueue, e => e.ConfigureConsumer<OrderCreatedEventConsumer>(context));
            });
            
        });
        
        builder.Services.AddSingleton<MongoDbService>();

        
        #region Harici- MongoDb'ye SeedData Ekeleme
        using IServiceScope scope = builder.Services.BuildServiceProvider().CreateScope();
        MongoDbService mongoDbService=scope.ServiceProvider.GetService<MongoDbService>();
        var collection = mongoDbService.GetCollection<Stock.API.Models.Entities.Stock>();
        if (!collection.FindSync(s => true).Any())
        {
            await collection.InsertOneAsync(new() { ProductId = "23849C0F-4FCC-6A45-A99C-BA65354909AF", Count = 2000 });
            await collection.InsertOneAsync(new() { ProductId = "13D188BB-5553-0D49-9EE4-666D3F0CF8D3", Count = 1000 });
            await collection.InsertOneAsync(new() { ProductId = "8E607A32-6388-444A-9A5C-28058E0EE836", Count = 3000 });
            await collection.InsertOneAsync(new() { ProductId = "4F874EB8-6153-C94B-BE8B-9BDA192CEEEB", Count = 5000 });
            await collection.InsertOneAsync(new() { ProductId = "AEACB7CA-9056-D44C-B590-3C46255F7E33", Count = 500 });
        }
        #endregion
        


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
    }
}
