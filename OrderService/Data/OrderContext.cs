using Microsoft.EntityFrameworkCore;

namespace OrderService.Data
{
    public class OrderContext : DbContext
    {
        protected OrderContext()
        {
        }

        public OrderContext(DbContextOptions options) 
            : base(options)
        {
        }

        public DbSet<OrderEntity> Orders { get; set; }
    }
}