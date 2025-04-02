using Microsoft.EntityFrameworkCore;
using FinanceTracker.API.Models;

namespace FinanceTracker.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(
                    "Your connection string", // This will be used only if not configured elsewhere
                    options => options
                        .CommandTimeout(180)
                        .EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorCodesToAdd: null)
                );
            }
            else
            {
                // Apply these options even if already configured
                optionsBuilder.UseNpgsql(
                    b => b.CommandTimeout(180)
                         .EnableRetryOnFailure(5)
                );
            }


        }

        public DbSet<LinkedItem> LinkedItems { get; set; } = null!;
        public DbSet<User> Users { get; set; }
        public DbSet<Budget> Budget { get; set; }
        public DbSet<Categories> Categories { get; set; }

    }
}
