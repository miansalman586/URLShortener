using Microsoft.EntityFrameworkCore;
using URLShortener.Entity;

namespace URLShortener.Data
{
    public class ContactDbContext : DbContext
    {
        public DbSet<Contact> Contact { get; set; }

        public ContactDbContext(DbContextOptions<ContactDbContext> options) : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Contact>();
        }
    }
}
