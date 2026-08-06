using Microsoft.EntityFrameworkCore;
using Persistence.Entities;
using SummerGameServer.DbContexts;
using SummerGameServer.Models.Entities;

namespace SummerGameServer.Tests;

public sealed class PersistenceModelTests
{
    private static UserDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<UserDbContext>()
            .UseMySql(
                "Server=localhost;Database=model_test;User=test;Password=test",
                new MySqlServerVersion(new Version(8, 0, 41)))
            .Options;
        return new UserDbContext(options);
    }

    [Fact]
    public void StageRun_MapsRewardAuditFieldsAndUserForeignKey()
    {
        using UserDbContext context = CreateContext();
        var entity = context.Model.FindEntityType(typeof(StageRun));

        Assert.NotNull(entity?.FindProperty(nameof(StageRun.ExpGained)));
        Assert.Contains(entity!.GetForeignKeys(), foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == typeof(User));
    }

    [Fact]
    public void UserRoom_HasUniqueUserIndexAndForeignKey()
    {
        using UserDbContext context = CreateContext();
        var entity = context.Model.FindEntityType(typeof(UserRoom));

        Assert.Contains(entity!.GetIndexes(), index =>
            index.IsUnique &&
            index.Properties.Count == 1 &&
            index.Properties[0].Name == nameof(UserRoom.UserId));
        Assert.Contains(entity.GetForeignKeys(), foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == typeof(User));
    }

    [Fact]
    public void LoginContext_DoesNotOwnCharacterSchema()
    {
        var options = new DbContextOptionsBuilder<SummerLoginServer.DbContexts.UserDbContext>()
            .UseMySql(
                "Server=localhost;Database=model_test;User=test;Password=test",
                new MySqlServerVersion(new Version(8, 0, 41)))
            .Options;
        using var context = new SummerLoginServer.DbContexts.UserDbContext(options);

        Assert.Null(context.Model.FindEntityType(typeof(Character)));
        Assert.NotNull(context.Model.FindEntityType(typeof(User)));
    }
}
