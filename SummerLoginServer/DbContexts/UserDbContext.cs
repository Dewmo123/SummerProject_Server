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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Username)
                    .IsUnique();

                entity.HasIndex(u => new
                {
                    u.Provider,
                    u.ProviderUserId
                }).IsUnique();

                entity.Property(u => u.ProviderUserId)
                    .HasMaxLength(255);
            });
        }
    }
}
