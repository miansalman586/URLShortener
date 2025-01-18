using Microsoft.EntityFrameworkCore;
using URLShortener.Entity;

namespace URLShortener.Data
{
    public class UserAgentDbContext : DbContext
    {
        public DbSet<UserAgent> UserAgent { get; set; }

        public UserAgentDbContext(DbContextOptions<UserAgentDbContext> options) : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserAgent>();
        }
    }
}
