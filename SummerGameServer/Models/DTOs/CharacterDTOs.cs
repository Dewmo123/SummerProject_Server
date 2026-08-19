using Persistence.Entities;

namespace SummerGameServer.Models.DTOs
{
    public record CharacterResponse
    {
        public int Level { get; set; }
        public long Exp { get; set; }
        public long ExpToNextLevel { get; set; }
        public static CharacterResponse FromModel(CharacterModel c, long expToNextLevel) => new CharacterResponse()
        {
            Level = c.Level,
            Exp = c.Exp,
            ExpToNextLevel = expToNextLevel
        };
    }
}
