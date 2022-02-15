using System;
using System.Data;
using System.Text;
using Newtonsoft.Json;
using OrderService.Events;
using RabbitMQ.Client;

namespace OrderService.RabbitMq
{
    public class RabbitMqService
    {
        public const string RabbitMqConnectionString =
            "amqps://bquhpfjz:GtYKmVmWJRlI-lTpXUrj3tKV9-tog2TG@cow.rmq2.cloudamqp.com/bquhpfjz";

        public void SendOrderCreatedEvent(OrderCreatedEvent ev)
        {
            var message = JsonConvert.SerializeObject(ev);
            var body = Encoding.UTF8.GetBytes(message); 

            var factory = new ConnectionFactory { Uri = new Uri(RabbitMqConnectionString) };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.BasicPublish("", "OrderCreatedEventQueue", null, body);
            Console.WriteLine("OrderCreatedEvent is sent to DefaultExchange!");
        }
    }
}