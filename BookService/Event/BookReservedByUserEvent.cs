namespace BookService.Event
{
    public class BookReservedByUserEvent
    {
        public int BookId { get; set; }
        public int CustomerId { get; set; }
        public int OrderId { get; set; }
        public decimal Price { get; set; }
    }
}