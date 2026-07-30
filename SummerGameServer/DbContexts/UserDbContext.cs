using Microsoft.EntityFrameworkCore;
using Persistence.Configuration;
using Persistence.Entities;
using SummerGameServer.Entities;

namespace SummerGameServer.DbContexts
{
    public class UserDbContext : DbContext
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Character> Characters => Set<Character>();
        public DbSet<CharacterCurrency> Currencies => Set<CharacterCurrency>();
        public DbSet<UserRoom> UserRooms => Set<UserRoom>();
        public DbSet<StageRun> StageRuns => Set<StageRun>();
        public UserDbContext(DbContextOptions options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new CharacterConfiguration());

            modelBuilder.Entity<UserRoom>()
                .Property(x => x.TrapData)
                .HasColumnType("json");
            modelBuilder.Entity<StageRun>()
                .HasIndex(run => new { run.UserId, run.Status }); //키인덱스로 UserId와 Status를 조합한다.
        }
    }
}
