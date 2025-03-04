
using Coordinator.Models.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Coordinator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<TwoPhaseCommitContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("SQLServer")));

            builder.Services.AddHttpClient("OrderAPI", client => client.BaseAddress = new("https://localhost:7232/"));
            builder.Services.AddHttpClient("StockAPI", client => client.BaseAddress = new("https://localhost:7252/"));
            builder.Services.AddHttpClient("PaymentAPI", client => client.BaseAddress = new("https://localhost:7009/"));



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
}
