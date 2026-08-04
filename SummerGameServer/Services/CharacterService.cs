using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Persistence.Entities;
using SummerGameServer.DbContexts;
using SummerGameServer.Models.DTOs;

namespace SummerGameServer.Services;

public sealed class CharacterService(UserDbContext dbContext)
{
    public async Task<Character?> GetOrCreateAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (!await dbContext.Users.AnyAsync(user => user.Id == userId, cancellationToken))
            return null;

        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT IGNORE INTO `Characters` (`UserId`, `Level`, `Exp`) VALUES ({userId}, 1, 0)",
            cancellationToken);

        return await dbContext.Characters
            .AsNoTracking()
            .SingleOrDefaultAsync(character => character.UserId == userId, cancellationToken);
    }

    public async Task<CharacterResponse?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        Character? character = await GetOrCreateAsync(userId, cancellationToken);
        return character is null
            ? null
            : CharacterResponse.FromEntity(character, Leveling.RequiredExp(character.Level));
    }

    public async Task<CharacterResponse?> AddExpAsync(int userId, long amount, CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "경험치는 양수여야 합니다.");

        IDbContextTransaction? ownedTransaction = null;
        if (dbContext.Database.CurrentTransaction is null)
            ownedTransaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"INSERT IGNORE INTO `Characters` (`UserId`, `Level`, `Exp`) SELECT {userId}, 1, 0 FROM `Users` WHERE `Id` = {userId}",
                cancellationToken);

            Character? character = await dbContext.Characters
                .FromSqlInterpolated($"SELECT * FROM `Characters` WHERE `UserId` = {userId} FOR UPDATE")
                .SingleOrDefaultAsync(cancellationToken);
            if (character is null)
                return null;

            character.Exp = checked(character.Exp + amount);
            while (character.Exp >= Leveling.RequiredExp(character.Level))
            {
                character.Exp -= Leveling.RequiredExp(character.Level);
                character.Level = checked(character.Level + 1);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            if (ownedTransaction is not null)
                await ownedTransaction.CommitAsync(cancellationToken);

            return CharacterResponse.FromEntity(character, Leveling.RequiredExp(character.Level));
        }
        catch
        {
            if (ownedTransaction is not null)
                await ownedTransaction.RollbackAsync(CancellationToken.None);
            throw;
        }
        finally
        {
            if (ownedTransaction is not null)
                await ownedTransaction.DisposeAsync();
        }
    }
}
