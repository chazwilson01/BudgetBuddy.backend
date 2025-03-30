using Microsoft.EntityFrameworkCore;
using FinanceTracker.API.Models;

namespace FinanceTracker.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<LinkedItem> LinkedItems { get; set; } = null!;
        public DbSet<User> Users { get; set; }
        public DbSet<Budget> Budget { get; set; }
        public DbSet<Categories> Categories { get; set; }

    }
}
