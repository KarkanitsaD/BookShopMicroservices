namespace BookService.RabbitMq
{
    public static class Queues
    {
        public const string RabbitMqConnectionString = "amqps://bquhpfjz:GtYKmVmWJRlI-lTpXUrj3tKV9-tog2TG@cow.rmq2.cloudamqp.com/bquhpfjz";

        public const string OrderCreatedEventQueue = "OrderCreatedEventQueue";

        public const string BookReservedByUserEventQueue = "BookReservedByUserEventQueue";

        public const string ReservationFailedEventQueue = "ReservationFailedEventQueue";

        public const string PaymentFailedEventQueue = "PaymentFailedEventQueue";
    }
}
