using Microsoft.EntityFrameworkCore;
using SummerGameServer.DbContexts;
using SummerGameServer.Models.DAOs;
using SummerGameServer.Models.DTOs;

namespace SummerGameServer.Services
{
    public enum CurrencyError
    {
        None = 0,
        UserNotFound,
        LackOfCurrency,
        InvalidCurrency
    }
    public class CurrencyService
    {
        private readonly UserDbContext _dbContext;
        public CurrencyService(UserDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<(CurrencyError error, CurrenciesResponse? response)> GetOrCreateAllAsync(int userId)
        {
            bool userExists = await _dbContext.Users.AnyAsync(user => user.Id == userId);
            if (!userExists)
                return (CurrencyError.UserNotFound, null);
            Dictionary<CurrencyType, Currency> currencies
                = _dbContext.Currencies.Where(currency => currency.UserId == userId)
                .ToDictionary(currency => currency.Type);
            foreach (var type in Enum.GetValues<CurrencyType>())
            {
                if (currencies.ContainsKey(type))
                    continue;
                Currency currency = new() { UserId = userId, Type = type };
                await _dbContext.Currencies.AddAsync(currency);
                currencies.Add(type, currency);
            }
            await _dbContext.SaveChangesAsync();
            return (CurrencyError.None, new CurrenciesResponse()
            {
                Currencies = currencies
                .ToDictionary(currencies => currencies.Key,
                currencies => currencies.Value.Amount)
            });
        }
        public async Task<(CurrencyError error, CurrencyResponse? response)> GetByUserIdAsync(int userId, CurrencyType type)
        {
            (CurrencyError error, Currency? currency) = await GetOrCreateAsync(userId,type);
            if (error != CurrencyError.None || currency is null)
                return (error, null);
            return (CurrencyError.None, new CurrencyResponse() { Amount = currency.Amount, Type = type });
        }
        public async Task<(CurrencyError error, Currency? currency)> GetOrCreateAsync(int userId, CurrencyType type)
        {
            if (!Enum.IsDefined(type))
                return (CurrencyError.InvalidCurrency, null);
            Currency? currency = await _dbContext.Currencies.SingleOrDefaultAsync(currency => currency.UserId == userId && currency.Type == type);
            if (currency != null)
                return (CurrencyError.None, currency);
            bool userExists = await _dbContext.Users.AnyAsync(user => user.Id == userId);
            if (!userExists)
                return (CurrencyError.UserNotFound, currency);

            currency = new Currency() { Type = type, UserId = userId };
            await _dbContext.Currencies.AddAsync(currency);
            await _dbContext.SaveChangesAsync();

            return (CurrencyError.None, currency);
        }
        public async Task<(CurrencyError error, Currency? currency)> AddAsync(int userId, CurrencyType type, long amount)
        {
            if (!Enum.IsDefined(type))
                return (CurrencyError.InvalidCurrency, null);
            var item = await GetOrCreateAsync(userId, type);
            if (item.error != CurrencyError.None || item.currency == null)
                return item;
            item.currency.Amount += Math.Abs(amount);
            await _dbContext.SaveChangesAsync();
            return item;
        }
        public async Task<(CurrencyError error, Currency? currency)> RemoveAsync(int userId, CurrencyType type, long amount)
        {
            if (!Enum.IsDefined(type))
                return (CurrencyError.InvalidCurrency, null);
            var item = await GetOrCreateAsync(userId, type);
            if (item.error != CurrencyError.None || item.currency == null)
                return item;
            long remain = item.currency.Amount - Math.Abs(amount);
            if (remain < 0)
                return (CurrencyError.LackOfCurrency, null);
            item.currency.Amount = remain;
            await _dbContext.SaveChangesAsync();
            return item;
        }

    }
}
