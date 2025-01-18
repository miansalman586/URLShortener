using Microsoft.EntityFrameworkCore;
using URLShortener.Entity;

namespace URLShortener.Data
{
    public class IPAddressDbContext : DbContext
    {
        public DbSet<IPAddress> IPAddress { get; set; }

        public IPAddressDbContext(DbContextOptions<IPAddressDbContext> options) : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IPAddress>();
        }
    }
}
