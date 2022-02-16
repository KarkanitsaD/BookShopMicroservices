using System;
using System.Data;
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
        private readonly IModel _channel;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public RabbitMqOrderCreatedEventListener(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;

            var factory = new ConnectionFactory { Uri = new Uri(Queues.RabbitMqConnectionString) };
            var connection = factory.CreateConnection();

            _channel = connection.CreateModel();
            _channel.QueueDeclare(
                queue: Queues.OrderCreatedEventQueue,
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
                queue: Queues.OrderCreatedEventQueue, 
                autoAck: true, 
                consumer: consumer);

            return Task.CompletedTask;
        }

        private async void ReceivedHandlerAsync(object sender, BasicDeliverEventArgs e)
        {
            await using var context = _serviceScopeFactory.CreateScope()
                .ServiceProvider.GetRequiredService<BookContext>();

            var orderCreatedEventString = Encoding.UTF8.GetString(e.Body.ToArray());
            var orderCreatedEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(orderCreatedEventString);

            await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead);
            try
            {
                var book = await context.Books.FirstOrDefaultAsync(b => b.Id == orderCreatedEvent.BookId);
                if (book is not { Quantity: > 0 })
                {
                    throw new Exception();
                }

                book.Quantity -= 1;
                context.Books.Update(book);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                var bookReservedEvent = new BookReservedByUserEvent
                {
                    BookId = book.Id,
                    CustomerId = orderCreatedEvent.CustomerId,
                    Price = book.Price
                };

                SendBookReservedByUserEvent(book, orderCreatedEvent);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                SendReservationFailedEvent(orderCreatedEvent);
            }
        }

        private void SendReservationFailedEvent(OrderCreatedEvent ev)
        {
            var reservationFailedEvent = new ReservationFailedEvent { OrderId = ev.OrderId };
            var reservationFailedEventString = JsonConvert.SerializeObject(reservationFailedEvent);
            var body = Encoding.UTF8.GetBytes(reservationFailedEventString);

            _channel.BasicPublish(
                exchange: "",
                routingKey: Queues.ReservationFailedEventQueue,
                basicProperties: null,
                body: body);
        }

        private void SendBookReservedByUserEvent(BookEntity book, OrderCreatedEvent ev)
        {
            var bookReservedByUserEvent = new BookReservedByUserEvent
            {
                BookId = book.Id,
                CustomerId = ev.CustomerId,
                OrderId = ev.OrderId,
                Price = book.Price
            };
            var bookReservedByUserEventString = JsonConvert.SerializeObject(bookReservedByUserEvent);
            var body = Encoding.UTF8.GetBytes(bookReservedByUserEventString);

            _channel.BasicPublish(
                exchange: "",
                routingKey: Queues.BookReservedByUserEventQueue,
                basicProperties: null,
                body: body);
        }
    }
}