using Concord.Domain.Models.Providers;
using Microsoft.EntityFrameworkCore;

namespace Concord.Domain.Context.Application
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        { }

        public DbSet<Provider> Providers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Provider:
            modelBuilder.Entity<Provider>(b =>
            {
                b.ToTable("Providers");
            });

        }
    }
}
