using Persistence.Entities;

namespace SummerGameServer.Entities
{
    public enum CurrencyType
    {
        Gold = 1,
        Gem = 2,
        StageTicket = 3,
        EventToken = 4,
    }

    public class Currency
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public CurrencyType Type { get; set; }
        public long Amount { get; set; } = 0;

    }
}
