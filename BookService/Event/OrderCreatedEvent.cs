namespace BookService.Event
{
    public class OrderCreatedEvent
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public int BookId { get; set; }
    }
}