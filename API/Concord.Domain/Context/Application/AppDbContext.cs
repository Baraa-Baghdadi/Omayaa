using Concord.Domain.Models.Categories;
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
        public DbSet<Subcategory> Subcategories { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Provider:
            modelBuilder.Entity<Provider>(b =>
            {
                b.ToTable("Providers");
            });

            // One-to-Many: Category -> Subcategories
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Subcategories)
                .WithOne(sc => sc.Category)
                .HasForeignKey(sc => sc.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-Many: Subcategory -> Products
            modelBuilder.Entity<Subcategory>()
                .HasMany(sc => sc.Products)
                .WithOne(p => p.Subcategory)
                .HasForeignKey(p => p.SubcategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
