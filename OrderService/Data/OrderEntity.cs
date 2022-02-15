using System;

namespace OrderService.Data
{
    public class OrderEntity
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int BookId { get; set; }
        public DateTime OrderTime { get; set; }
    }
}