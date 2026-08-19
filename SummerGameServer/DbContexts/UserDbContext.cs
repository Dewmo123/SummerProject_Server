using Microsoft.EntityFrameworkCore;
using Persistence.Configuration;
using Persistence.Entities;
using SummerGameServer.Models.Entities;

namespace SummerGameServer.DbContexts
{
    public class UserDbContext : DbContext
    {
        public DbSet<UserModel> Users => Set<UserModel>();
        public DbSet<CharacterModel> Characters => Set<CharacterModel>();
        public DbSet<CurrencyModel> Currencies => Set<CurrencyModel>();
        public DbSet<UserRoomModel> UserRooms => Set<UserRoomModel>();
        public DbSet<StageRunModel> StageRuns => Set<StageRunModel>();
        public UserDbContext(DbContextOptions options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new CharacterConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());

            // Users 스키마와 마이그레이션은 로그인 서버가 단독 소유한다.
            modelBuilder.Entity<UserModel>()
                .ToTable("Users", table => table.ExcludeFromMigrations());
            modelBuilder.Entity<CharacterModel>()
                .ToTable(table => table.HasCheckConstraint(
                    "CK_Characters_Level_Exp",
                    "`Level` >= 1 AND `Exp` >= 0"));

            modelBuilder.Entity<UserRoomModel>()
                .Property(x => x.TrapData)
                .HasColumnType("json");
            modelBuilder.Entity<UserRoomModel>()
                .HasIndex(room => room.UserId)
                .IsUnique();
            modelBuilder.Entity<UserRoomModel>()
                .HasOne(room => room.User)
                .WithOne()
                .HasForeignKey<UserRoomModel>(room => room.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<StageRunModel>()
                .HasIndex(run => new { run.UserId, run.Status }); //키인덱스로 UserId와 Status를 조합한다.
            modelBuilder.Entity<StageRunModel>()
                .HasOne(run => run.User)
                .WithMany()
                .HasForeignKey(run => run.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<CurrencyModel>()
                .HasKey(currency => new { currency.UserId, currency.Type });
            modelBuilder.Entity<CurrencyModel>()
                .ToTable(table => table.HasCheckConstraint(
                    "CK_Currencies_Amount",
                    "`Amount` >= 0"));
        }
    }
}
