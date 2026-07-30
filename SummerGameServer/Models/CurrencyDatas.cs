using SummerGameServer.Entities;

namespace SummerGameServer.Models
{
    public record CurrenciesResponse
    {
        public Dictionary<CurrencyType, long> Currencies { get; set; } = new();
    }
    public record GainCurrencyRequest
    {
        public long Amount;
        public CurrencyType Type { get; set; }
    }
}
