using Microsoft.EntityFrameworkCore;
using Persistence.Entities;
using Persistence.Configuration;

namespace SummerLoginServer.DbContexts
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {

        }
        public DbSet<User> Users => Set<User>();
        public DbSet<Character> Chars => Set<Character>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new CharacterConfiguration());
        }
    }
}
