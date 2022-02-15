using Microsoft.EntityFrameworkCore;

namespace CustomerService.Data
{
    public class CustomerContext : DbContext
    {
        protected CustomerContext()
        {
        }

        public CustomerContext(DbContextOptions options) 
            : base(options)
        {
        }

        public DbSet<CustomerEntity> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
        }
    }
}