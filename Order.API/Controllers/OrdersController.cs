using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.API.Models;
using Order.API.Models.Enums;
using Order.API.Models.ViewModels;
using Shared.Events;

namespace Order.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        readonly OrderAPIDbContext _orderAPIDbContext;
        readonly IPublishEndpoint _publishEndpoint;

        public OrdersController(OrderAPIDbContext orderAPIDbContext, IPublishEndpoint publishEndpoint)
        {
            _orderAPIDbContext = orderAPIDbContext;
            _publishEndpoint = publishEndpoint;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(CreateOrderVM model)
        {
            Order.API.Models.Entities.Order order = new()
            {
                Id = Guid.NewGuid(),
                BuyerId = model.BuyerId,
                CreatedDate = DateTime.Now,
                Status = OrderStatus.Suspend
            };
            order.OrderItems= model.OrderItems.Select(oi=> new Models.Entities.OrderItem
            {
                Count= oi.Count,
                Price= oi.Price,
                ProductId= oi.ProductId
            }).ToList();

            order.TotalPrice = order.OrderItems.Sum(oi => oi.Price * oi.Count);

            await _orderAPIDbContext.AddAsync(order);
            await _orderAPIDbContext.SaveChangesAsync();

            OrderCreatedEvent orderCreatedEvent = new()
            {
                BuyerId = order.BuyerId,
                OrderId = order.Id,
                OrderItems = order.OrderItems.Select(oi => new Shared.Messages.OrderItemMessage
                {
                    Count=oi.Count,
                    ProductId=oi.ProductId
                }).ToList(),
                TotalPrice=order.TotalPrice
            };
            await _publishEndpoint.Publish(orderCreatedEvent);
            return Ok();
        }
    }
}
