using Persistence.Entities;
using SummerGameServer.Entities;
using System.ComponentModel.DataAnnotations;

namespace SummerGameServer.Models
{
    public record CharacterResponse
    {
        public int Level { get; set; }
        public long Exp { get; set; }
        public long ExpToNextLevel { get; set; }
        public static CharacterResponse FromEntity(Character c, long expToNextLevel) => new CharacterResponse()
        {
            Level = c.Level,
            Exp = c.Exp,
            ExpToNextLevel = expToNextLevel
        };
    }
    public class GainExpRequest
    {
        [Range(1, 1_000_000, ErrorMessage = "경험치는 1 이상이어야 합니다.")]
        public int Amount { get; set; }
    }
}
