using SummerGameServer.Entities;

namespace SummerGameServer.Models
{
    public record CurrenciesResponse
    {
        public Dictionary<CurrencyType, long> Currencies { get; set; } = new();
    }
}
