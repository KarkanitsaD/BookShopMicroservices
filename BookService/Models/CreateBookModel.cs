namespace BookService.Models
{
    public class CreateBookModel
    {
        public string Title { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}