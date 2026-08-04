using Microsoft.EntityFrameworkCore;
using Persistence.Entities;
using SummerGameServer.DbContexts;
using SummerGameServer.Models.DTOs;

namespace SummerGameServer.Services
{
    public class CharacterService
    {
        private readonly UserDbContext _dbContext;
        public CharacterService(UserDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<Character?> GetOrCreateAsync(int userId)
        {
            Character? character = await _dbContext.Characters.FirstOrDefaultAsync(c => c.UserId == userId);
            if (character is not null)
                return character;
            bool userExists = await _dbContext.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
                return null;
            character = new Character { UserId = userId };
            _dbContext.Characters.Add(character);
            await _dbContext.SaveChangesAsync();
            return character;
        }
        public async Task<CharacterResponse?> GetByUserIdAsync(int userId)
        {
            Character? character = await GetOrCreateAsync(userId);
            return character is null ? null : CharacterResponse.FromEntity(character, Leveling.RequiredExp(character.Level));
        }
        public async Task<CharacterResponse?> AddExpAsync(int userId,int amount)
        {
            Character? character = await GetOrCreateAsync(userId);
            if (character is null)
                return null;
            character.Exp += amount;
            while(character.Exp >= Leveling.RequiredExp(character.Level))
            {
                character.Exp -= Leveling.RequiredExp(character.Level);
                character.Level++;
            }
            await _dbContext.SaveChangesAsync();
            return CharacterResponse.FromEntity(character, Leveling.RequiredExp(character.Level));
        }
    }
}
