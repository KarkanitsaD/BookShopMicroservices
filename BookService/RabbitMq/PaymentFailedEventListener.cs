using System;
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
    public class PaymentFailedEventListener : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IModel _channel;

        public PaymentFailedEventListener(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;

            var factory = new ConnectionFactory { Uri = new Uri(Queues.RabbitMqConnectionString) };
            var connection = factory.CreateConnection();

            _channel = connection.CreateModel();
            _channel.QueueDeclare(
                queue: Queues.PaymentFailedEventQueue,
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
                queue: Queues.PaymentFailedEventQueue,
                autoAck: true,
                consumer: consumer);

            return Task.CompletedTask;
        }

        private async void ReceivedHandlerAsync(object sender, BasicDeliverEventArgs e)
        {
            await using var context = _serviceScopeFactory.CreateScope()
                .ServiceProvider.GetRequiredService<BookContext>();

            var paymentFailedEventString = Encoding.UTF8.GetString(e.Body.ToArray());
            var paymentFailedEvent = JsonSerializer.Deserialize<PaymentFailedEvent>(paymentFailedEventString);

            var book = await context.Books.FirstOrDefaultAsync(b => b.Id == paymentFailedEvent.BookId);
            book.Quantity += 1;
            context.Books.Update(book);
            await context.SaveChangesAsync();

            SendReservationFailedEvent(paymentFailedEvent.OrderId);
        }

        private void SendReservationFailedEvent(int orderId)
        {
            var reservationFailedEvent = new ReservationFailedEvent { OrderId = orderId };
            var reservationFailedEventString = JsonConvert.SerializeObject(reservationFailedEvent);
            var body = Encoding.UTF8.GetBytes(reservationFailedEventString);

            _channel.BasicPublish(
                exchange: "",
                routingKey: Queues.ReservationFailedEventQueue,
                basicProperties: null,
                body: body);
        }
    }
}