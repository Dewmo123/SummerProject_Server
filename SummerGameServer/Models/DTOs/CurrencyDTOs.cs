using SummerGameServer.Models.Entities;

namespace SummerGameServer.Models.DTOs
{
    public record CurrenciesResponse
    {
        public Dictionary<CurrencyType, long> Currencies { get; set; } = new();
    }
    public record CurrencyResponse
    {
        public CurrencyType Type { get; set; }
        public long Amount { get; set; }
    }
}
