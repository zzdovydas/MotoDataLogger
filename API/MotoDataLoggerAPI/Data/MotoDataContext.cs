using MotoDataLoggerAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace MotoDataLoggerAPI.Data
{
    public class MotoDataContext : DbContext
    {
        //Correct constructor name and parameter type.
        public MotoDataContext(DbContextOptions<MotoDataContext> options)
            : base(options)
        {
        }
        public DbSet<ApiKey> ApiKeys { get; set; }
        public DbSet<Motorcycle> Motorcycles { get; set; }
        public DbSet<MotoData> MotoDatas { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Fluent API to set up relationships

            modelBuilder.Entity<ApiKey>()
                .HasOne(a => a.Motorcycle)
                .WithMany(m => m.ApiKeys)
                .HasForeignKey(a => a.MotorcycleId);

            modelBuilder.Entity<MotoData>()
                .HasOne(md => md.ApiKey)
                .WithMany()
                .IsRequired(false);
            
        }
    }
}
