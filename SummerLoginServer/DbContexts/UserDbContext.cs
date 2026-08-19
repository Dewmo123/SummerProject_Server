using Microsoft.EntityFrameworkCore;
using Persistence.Configuration;
using Persistence.Entities;

namespace SummerLoginServer.DbContexts
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {

        }
        public DbSet<UserModel> Users => Set<UserModel>();
        public DbSet<RefreshTokenModel> RefreshTokens => Set<RefreshTokenModel>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
            modelBuilder.Ignore<CharacterModel>();
        }
    }
}
