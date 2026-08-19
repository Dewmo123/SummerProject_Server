using Microsoft.EntityFrameworkCore;
using SummerGameServer.DbContexts;
using SummerGameServer.Models.Entities;
using SummerGameServer.Models.DTOs;

namespace SummerGameServer.Services;

public enum CurrencyError
{
    None = 0,
    UserNotFound,
    LackOfCurrency,
    InvalidCurrency,
    InvalidAmount,
    Overflow
}

public sealed class CurrencyService(UserDbContext dbContext)
{
    public async Task<(CurrencyError error, CurrenciesResponse? response)> GetOrCreateAllAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        if (!await UserExistsAsync(userId, cancellationToken))
            return (CurrencyError.UserNotFound, null);

        foreach (CurrencyType type in Enum.GetValues<CurrencyType>())
        {
            await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"INSERT IGNORE INTO `Currencies` (`UserId`, `Type`, `Amount`) VALUES ({userId}, {(int)type}, 0)",
                cancellationToken);
        }

        Dictionary<CurrencyType, long> currencies = await dbContext.Currencies
            .AsNoTracking()
            .Where(currency => currency.UserId == userId)
            .ToDictionaryAsync(currency => currency.Type, currency => currency.Amount, cancellationToken);

        return (CurrencyError.None, new CurrenciesResponse { Currencies = currencies });
    }

    public async Task<(CurrencyError error, CurrencyResponse? response)> GetByUserIdAsync(
        int userId,
        CurrencyType type,
        CancellationToken cancellationToken = default)
    {
        (CurrencyError error, CurrencyModel? currency) = await GetOrCreateAsync(userId, type, cancellationToken);
        return error != CurrencyError.None || currency is null
            ? (error, null)
            : (CurrencyError.None, new CurrencyResponse { Amount = currency.Amount, Type = type });
    }

    public async Task<(CurrencyError error, CurrencyModel? currency)> GetOrCreateAsync(
        int userId,
        CurrencyType type,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(type))
            return (CurrencyError.InvalidCurrency, null);
        if (!await UserExistsAsync(userId, cancellationToken))
            return (CurrencyError.UserNotFound, null);

        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT IGNORE INTO `Currencies` (`UserId`, `Type`, `Amount`) VALUES ({userId}, {(int)type}, 0)",
            cancellationToken);

        CurrencyModel? currency = await FindAsync(userId, type, cancellationToken);
        return currency is null
            ? (CurrencyError.UserNotFound, null)
            : (CurrencyError.None, currency);
    }

    public async Task<(CurrencyError error, CurrencyModel? currency)> AddAsync(
        int userId,
        CurrencyType type,
        long amount,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(type))
            return (CurrencyError.InvalidCurrency, null);
        if (amount <= 0)
            return (CurrencyError.InvalidAmount, null);

        (CurrencyError error, CurrencyModel? currency) = await GetOrCreateAsync(userId, type, cancellationToken);
        if (error != CurrencyError.None || currency is null)
            return (error, null);

        int affected = await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE `Currencies` SET `Amount` = `Amount` + {amount} WHERE `UserId` = {userId} AND `Type` = {(int)type} AND `Amount` <= {long.MaxValue - amount}",
            cancellationToken);

        return affected == 1
            ? (CurrencyError.None, await FindAsync(userId, type, cancellationToken))
            : (CurrencyError.Overflow, null);
    }

    public async Task<(CurrencyError error, CurrencyModel? currency)> RemoveAsync(
        int userId,
        CurrencyType type,
        long amount,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(type))
            return (CurrencyError.InvalidCurrency, null);
        if (amount <= 0)
            return (CurrencyError.InvalidAmount, null);

        (CurrencyError error, CurrencyModel? currency) = await GetOrCreateAsync(userId, type, cancellationToken);
        if (error != CurrencyError.None || currency is null)
            return (error, null);

        int affected = await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE `Currencies` SET `Amount` = `Amount` - {amount} WHERE `UserId` = {userId} AND `Type` = {(int)type} AND `Amount` >= {amount}",
            cancellationToken);

        return affected == 1
            ? (CurrencyError.None, await FindAsync(userId, type, cancellationToken))
            : (CurrencyError.LackOfCurrency, null);
    }

    private Task<bool> UserExistsAsync(int userId, CancellationToken cancellationToken) =>
        dbContext.Users.AnyAsync(user => user.Id == userId, cancellationToken);

    private Task<CurrencyModel?> FindAsync(int userId, CurrencyType type, CancellationToken cancellationToken) =>
        dbContext.Currencies.AsNoTracking().SingleOrDefaultAsync(
            currency => currency.UserId == userId && currency.Type == type,
            cancellationToken);
}
