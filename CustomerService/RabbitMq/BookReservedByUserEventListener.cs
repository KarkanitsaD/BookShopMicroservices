using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CustomerService.Data;
using CustomerService.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace CustomerService.RabbitMq
{
    public class BookReservedByUserEventListener : BackgroundService
    {
        public const string RabbitMqConnectionString =
            "amqps://bquhpfjz:GtYKmVmWJRlI-lTpXUrj3tKV9-tog2TG@cow.rmq2.cloudamqp.com/bquhpfjz";



        private readonly IConnection _connection;
        private readonly IModel _channel;
        private IServiceScopeFactory _serviceScopeFactory;

        public BookReservedByUserEventListener(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
            var factory = new ConnectionFactory { Uri = new Uri(RabbitMqConnectionString) };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "BookReservedByUserEventQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (sender, ev) =>
            {
                CustomerContext context = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<CustomerContext>();

                var bookReservedByUserEventString = Encoding.UTF8.GetString(ev.Body.ToArray());
                var bookReservedByUserEvent =
                    JsonSerializer.Deserialize<BookReservedByUserEvent>(bookReservedByUserEventString);

                var customer =
                    await context.Customers.FirstOrDefaultAsync(c => c.Id == bookReservedByUserEvent.CustomerId);

                if (customer != null && customer.Account - bookReservedByUserEvent.Price > 0)
                {
                    customer.Account -= bookReservedByUserEvent.Price;
                    context.Customers.Update(customer);
                    await context.SaveChangesAsync();
                    Console.WriteLine("Order created, book reserved, book is pending!");
                    _channel.BasicAck(ev.DeliveryTag, false);
                }
            };
            _channel.BasicConsume("BookReservedByUserEventQueue", false, consumer);
        }
    }
}