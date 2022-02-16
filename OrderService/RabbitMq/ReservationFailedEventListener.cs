using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderService.Data;
using OrderService.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OrderService.RabbitMq
{
    public class ReservationFailedEventListener : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IModel _channel;

        public ReservationFailedEventListener(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;

            var factory = new ConnectionFactory { Uri = new Uri(Queues.RabbitMqConnectionString) };
            var connection = factory.CreateConnection();

            _channel = connection.CreateModel();
            _channel.QueueDeclare(
                queue: Queues.ReservationFailedEventQueue,
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
                queue: Queues.ReservationFailedEventQueue,
                autoAck: true,
                consumer: consumer);

            return Task.CompletedTask;
        }



        private async void ReceivedHandlerAsync(object sender, BasicDeliverEventArgs e)
        {
            var context = _serviceScopeFactory.CreateScope()
                .ServiceProvider.GetRequiredService<OrderContext>();

            var reservationFailedEventString = Encoding.UTF8.GetString(e.Body.ToArray());
            var reservationFailedEvent= JsonSerializer.Deserialize<ReservationFailedEvent>(reservationFailedEventString);

            var order = await context.Orders.FirstOrDefaultAsync(o => o.Id == reservationFailedEvent.OrderId);
            if (order != null)
            {
                context.Orders.Remove(order);
                await context.SaveChangesAsync();
                Console.WriteLine($"Order canceled {order.Id}");

            }
        }
    }
}