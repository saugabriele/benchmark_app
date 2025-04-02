using Microsoft.EntityFrameworkCore;
using DBA;

namespace DAL
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<UserModel> Users { get; set; }
        public DbSet<FileModel> Files { get; set; }
    }
}
