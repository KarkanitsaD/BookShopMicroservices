using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OrderService.Data;
using OrderService.Events;
using OrderService.Models;
using OrderService.RabbitMq;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderContext _context;
        private readonly RabbitMqService _rabbitMqService;

        public OrdersController(OrderContext context, RabbitMqService rabbitMqService)
        {
            _context = context;
            _rabbitMqService = rabbitMqService;
        }

        [HttpPost]
        public async Task CreateOrder([FromBody] CreateOrderModel order)
        {
            var orderEntity = new OrderEntity() { CustomerId = order.CustomerId, BookId = order.BookId, OrderTime = DateTime.Now };
            var entity = await _context.Orders.AddAsync(orderEntity);
            await _context.SaveChangesAsync();

            var orderCreatedEvent = new OrderCreatedEvent { BookId = entity.Entity.BookId, CustomerId = entity.Entity.CustomerId, OrderId = entity.Entity.Id };

            _rabbitMqService.SendOrderCreatedEvent(orderCreatedEvent);
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            return Ok(await _context.Orders.ToListAsync());
        }
    }
}