using Persistence.Entities;

namespace SummerGameServer.Models.Entities
{
    public enum CurrencyType
    {
        Gold = 1,
        Gem = 2,
        StageTicket = 3,
        EventToken = 4,
    }

    public class CurrencyModel
    {
        public int UserId { get; set; }
        public UserModel User { get; set; } = null!;
        public CurrencyType Type { get; set; }
        public long Amount { get; set; } = 0;
    }
}
