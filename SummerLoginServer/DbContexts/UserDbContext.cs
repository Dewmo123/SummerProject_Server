using Microsoft.EntityFrameworkCore;
using SummerLoginServer.Entities;

namespace SummerLoginServer.DbContexts
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {

        }
        public DbSet<User> Users => Set<User>();
        public DbSet<ExternalLogin> ExternalLogin => Set<ExternalLogin>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
            modelBuilder.Entity<ExternalLogin>()
                .HasIndex(u => new 
                {
                    u.Provider,
                    u.ProviderUserId
                })
                .IsUnique();
        }
    }
}
