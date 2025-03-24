using Microsoft.EntityFrameworkCore;
using SQLiteDb.Models;

namespace SQLiteDb.Repositories
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<FileRecord> Files { get; set; }
    }
}
