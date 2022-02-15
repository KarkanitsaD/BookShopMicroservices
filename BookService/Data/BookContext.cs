using Microsoft.EntityFrameworkCore;

namespace BookService.Data
{
    public class BookContext : DbContext
    {
        protected BookContext()
        {
        }

        public BookContext(DbContextOptions options) 
            : base(options)
        {
        }

        public DbSet<BookEntity> Books { get; set; }
    }
}