namespace CustomerService.RabbitMq
{
    public static class Queues
    {
        public const string RabbitMqConnectionString =
            "amqps://bquhpfjz:GtYKmVmWJRlI-lTpXUrj3tKV9-tog2TG@cow.rmq2.cloudamqp.com/bquhpfjz";

        public const string BookReservedByUserEventQueue = "BookReservedByUserEventQueue";

        public const string PaymentFailedEventQueue = "PaymentFailedEventQueue";
    }
}