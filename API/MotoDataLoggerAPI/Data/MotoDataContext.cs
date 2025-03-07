using Microsoft.EntityFrameworkCore;
using MotoDataLoggerAPI.Models;

namespace MotoDataLoggerAPI
{
    public class MotoDataContext : DbContext
    {
        public MotoDataContext(DbContextOptions<MotoDataContext> options) : base(options) { }

        public DbSet<Motorcycle> Motorcycles { get; set; }
        public DbSet<MotoData> MotoDatas { get; set; }
        public DbSet<User> Users {get; set;}
    }
}
