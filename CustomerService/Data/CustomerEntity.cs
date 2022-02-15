using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerService.Data
{
    public class CustomerEntity
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public decimal Account { get; set; }
    }

    public class CustomerEntityConfiguration : IEntityTypeConfiguration<CustomerEntity>
    {
        public void Configure(EntityTypeBuilder<CustomerEntity> builder)
        {
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Email)
                .IsRequired();
            builder.Property(b => b.Account);
        }
    }
}