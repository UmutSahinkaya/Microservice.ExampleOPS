using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Models;
using Shared.Events;

namespace Order.API.Consumers
{
    public class StockNotReservedEventConsumer : IConsumer<StockNotReservedEvent>
    {
        readonly OrderAPIDbContext _dbContext;

        public StockNotReservedEventConsumer(OrderAPIDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Consume(ConsumeContext<StockNotReservedEvent> context)
        {
            Order.API.Models.Entities.Order order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == context.Message.OrderId);
            order.Status = Models.Enums.OrderStatus.Failed;
            await _dbContext.SaveChangesAsync();
        }
    }
}
