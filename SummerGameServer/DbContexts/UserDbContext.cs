using Microsoft.EntityFrameworkCore;
using Persistence.Configuration;
using Persistence.Entities;
using SummerGameServer.Models.DAOs;

namespace SummerGameServer.DbContexts
{
    public class UserDbContext : DbContext
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Character> Characters => Set<Character>();
        public DbSet<Currency> Currencies => Set<Currency>();
        public DbSet<UserRoom> UserRooms => Set<UserRoom>();
        public DbSet<StageRun> StageRuns => Set<StageRun>();
        public UserDbContext(DbContextOptions options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new CharacterConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());

            modelBuilder.Entity<UserRoom>()
                .Property(x => x.TrapData)
                .HasColumnType("json");
            modelBuilder.Entity<StageRun>()
                .HasIndex(run => new { run.UserId, run.Status }); //키인덱스로 UserId와 Status를 조합한다.
            modelBuilder.Entity<Currency>()
                .HasKey(currency => new { currency.UserId, currency.Type });
        }
    }
}
