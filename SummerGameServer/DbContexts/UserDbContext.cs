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

            // Users 스키마와 마이그레이션은 로그인 서버가 단독 소유한다.
            modelBuilder.Entity<User>()
                .ToTable("Users", table => table.ExcludeFromMigrations());
            modelBuilder.Entity<Character>()
                .ToTable(table => table.HasCheckConstraint(
                    "CK_Characters_Level_Exp",
                    "`Level` >= 1 AND `Exp` >= 0"));

            modelBuilder.Entity<UserRoom>()
                .Property(x => x.TrapData)
                .HasColumnType("json");
            modelBuilder.Entity<UserRoom>()
                .HasIndex(room => room.UserId)
                .IsUnique();
            modelBuilder.Entity<UserRoom>()
                .HasOne(room => room.User)
                .WithOne()
                .HasForeignKey<UserRoom>(room => room.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<StageRun>()
                .HasIndex(run => new { run.UserId, run.Status }); //키인덱스로 UserId와 Status를 조합한다.
            modelBuilder.Entity<StageRun>()
                .HasOne(run => run.User)
                .WithMany()
                .HasForeignKey(run => run.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Currency>()
                .HasKey(currency => new { currency.UserId, currency.Type });
            modelBuilder.Entity<Currency>()
                .ToTable(table => table.HasCheckConstraint(
                    "CK_Currencies_Amount",
                    "`Amount` >= 0"));
        }
    }
}
