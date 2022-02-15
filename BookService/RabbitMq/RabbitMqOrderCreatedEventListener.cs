using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BookService.Data;
using BookService.Event;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace BookService.RabbitMq
{
    public class RabbitMqOrderCreatedEventListener : BackgroundService
    {
        public const string RabbitMqConnectionString =
            "amqps://bquhpfjz:GtYKmVmWJRlI-lTpXUrj3tKV9-tog2TG@cow.rmq2.cloudamqp.com/bquhpfjz";

        private readonly IConnection _connection;
        private readonly IModel _channel;
        private IServiceScopeFactory _serviceScopeFactory;

        public RabbitMqOrderCreatedEventListener(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
            var factory = new ConnectionFactory { Uri = new Uri(RabbitMqConnectionString) };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "OrderCreatedEventQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (sender, ev) =>
            {
                using var scope = _serviceScopeFactory.CreateScope();

                BookContext context = scope.ServiceProvider.GetRequiredService<BookContext>();

                Console.WriteLine("Received message!");
                var orderCreatedEventString = Encoding.UTF8.GetString(ev.Body.ToArray());

                var orderCreatedEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(orderCreatedEventString);

                var book =  context.Books.FirstOrDefault(b => b.Id == orderCreatedEvent.BookId);
                if (book is { Quantity: > 0 })
                {
                    book.Quantity -= 1;
                    context.Books.Update(book);
                     context.SaveChangesAsync();

                    var bookReservedEvent = new BookReservedByUserEvent
                    {
                        BookId = book.Id,
                        OrderId = orderCreatedEvent.OrderId,
                        CustomerId = orderCreatedEvent.CustomerId,
                        Price = book.Price
                    };

                    var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(bookReservedEvent));

                    _channel.BasicPublish("", "BookReservedByUserEventQueue", null, body);
                    _channel.BasicAck(ev.DeliveryTag, false);
                }
            };
            _channel.BasicConsume("OrderCreatedEventQueue", false, consumer);
        }
    }
}