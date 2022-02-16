using System;
using System.Data;
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
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IModel _channel;

        public BookReservedByUserEventListener(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;

            var factory = new ConnectionFactory { Uri = new Uri(Queues.RabbitMqConnectionString) };
            var connection = factory.CreateConnection();

            _channel = connection.CreateModel();
            _channel.QueueDeclare(
                queue: Queues.BookReservedByUserEventQueue,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += ReceivedHandlerAsync;

            _channel.BasicConsume(
                queue: Queues.BookReservedByUserEventQueue, 
                autoAck: true, 
                consumer: consumer);

            return Task.CompletedTask;
        }

        private async void ReceivedHandlerAsync(object sender, BasicDeliverEventArgs e)
        {
            await using var context = _serviceScopeFactory.CreateScope()
                .ServiceProvider.GetRequiredService<CustomerContext>();

            var bookReservedByUserEventString = Encoding.UTF8.GetString(e.Body.ToArray());
            var bookReservedByUserEvent =
                JsonSerializer.Deserialize<BookReservedByUserEvent>(bookReservedByUserEventString);

            await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead);
            try
            {
                var customer = await context.Customers.FirstOrDefaultAsync(c => c.Id == bookReservedByUserEvent.CustomerId);
                if (customer == null || customer.Account - bookReservedByUserEvent.Price < 0)
                {
                    throw new Exception();
                }

                customer.Account -= bookReservedByUserEvent.Price;
                context.Customers.Update(customer);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                SendPaymentFailedEvent(bookReservedByUserEvent);
            }
        }

        private void SendPaymentFailedEvent(BookReservedByUserEvent ev)
        {
            var paymentFailedEvent = new PaymentFailedEvent { BookId = ev.BookId, OrderId = ev.OrderId };
            var paymentFailedEventString = JsonConvert.SerializeObject(paymentFailedEvent);
            var body = Encoding.UTF8.GetBytes(paymentFailedEventString);

            _channel.BasicPublish(
                exchange: "",
                routingKey: Queues.PaymentFailedEventQueue,
                basicProperties: null,
                body: body);
        }
    }
}