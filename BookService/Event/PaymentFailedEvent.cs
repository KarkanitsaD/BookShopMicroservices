namespace BookService.Event
{
    public class PaymentFailedEvent
    {
        public int BookId { get; set; }
        public int OrderId { get; set; }
    }
}