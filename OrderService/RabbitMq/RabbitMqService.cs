using System;
using System.Text;
using Newtonsoft.Json;
using OrderService.Events;
using RabbitMQ.Client;

namespace OrderService.RabbitMq
{
    public class RabbitMqService
    {
        public void SendOrderCreatedEvent(OrderCreatedEvent ev)
        {
            var message = JsonConvert.SerializeObject(ev);
            var body = Encoding.UTF8.GetBytes(message); 

            var factory = new ConnectionFactory { Uri = new Uri(Queues.RabbitMqConnectionString) };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.BasicPublish(
                exchange: "", 
                routingKey: Queues.OrderCreatedEventQueue,
                basicProperties: null, 
                body: body);
        }
    }
}