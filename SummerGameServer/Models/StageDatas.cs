using Persistence.Entities;
using SummerGameServer.Entities;
using SummerGameServer.Services;

namespace SummerGameServer.Models
{
    //나중가면 추가될수 있는 필드들
    //Reward
    public sealed record StageData : ICatalogModel
    {
        public int Id => StageId;
        public int StageId { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool[] TileDatas { get; set; } = null!;
        public TrapData[] Trapdatas { get; set; } = null!;
    }
    public sealed record GetStageResponse(int Width, int Height, bool[] TileDatas, TrapData[] TrapDatas);
    public sealed record StageEnterResponse()
    {
        public int RunId { get; set; }
        public StageData StageData { get; set; } = null!;
        public static StageEnterResponse From(int runId, StageData stage, CatalogManager catalog)
        {
            //나중에 TrapCatalog 추가하면 그때 무결성 검사
            return new StageEnterResponse()
            {
                RunId = runId,
                StageData = stage,
            };
        }

    }
    public sealed record StageResultRequest()
    {
    }
    public sealed record StageResultResponse()
    {
        public int StageId { get; set; }
        public long ExpGained { get; set; }
        public CharacterResponse Character { get; set; } = null!;
        public CurrenciesResponse GainCurrencies { get; set; } = new();
        public CurrenciesResponse AllCurrencies { get; set; } = new();
    }
}
