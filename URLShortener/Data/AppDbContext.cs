using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.EntityFrameworkCore;
using URLShortener.Entity;

namespace URLShortener.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<ShortedURL> ShortedURL { get; set; }
        public DbSet<LangTrans> LangTrans { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ShortedURL>();
            modelBuilder.Entity<LangTrans>();  
        }
    }
}
