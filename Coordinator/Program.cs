
using Coordinator.Models.Contexts;
using Coordinator.Services.Abstraction;
using Microsoft.EntityFrameworkCore;

namespace Coordinator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<TwoPhaseCommitContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("SQLServer")));

            builder.Services.AddHttpClient("OrderAPI", client => client.BaseAddress = new("https://localhost:7232/"));
            builder.Services.AddHttpClient("StockAPI", client => client.BaseAddress = new("https://localhost:7252/"));
            builder.Services.AddHttpClient("PaymentAPI", client => client.BaseAddress = new("https://localhost:7009/"));

            builder.Services.AddSingleton<ITransactionService, ITransactionService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.MapGet("/create-order-transaction", async (ITransactionService transactionService) =>
            {
                // Phase 1 - Prepare
                var transactionId = await transactionService.CreateTransactionAsync();
                await transactionService.PrepareServicesAsync(transactionId);
                bool transactionState = await transactionService.CheckReadyServicesAsync(transactionId);

                if (transactionState)
                {
                    //Phase 2 - Commit
                    await transactionService.CommitAsync(transactionId);
                    transactionState = await transactionService.CheckTransactionStateServicesAsync(transactionId);
                }
                if (!transactionState)
                    await transactionService.RollBackAsync(transactionId);
            });

            app.Run();
        }
    }
}
