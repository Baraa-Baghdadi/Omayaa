using Concord.Domain.Models.Categories;
using Concord.Domain.Models.Orders;
using Concord.Domain.Models.Products;
using Concord.Domain.Models.Providers;
using Microsoft.EntityFrameworkCore;

namespace Concord.Domain.Context.Application
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        { }

        public DbSet<Provider> Providers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Provider:
            modelBuilder.Entity<Provider>(b =>
            {
                b.ToTable("Providers");
            });

            // Category - Product: one-to-many
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Products)
                .WithOne(p => p.Category)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
