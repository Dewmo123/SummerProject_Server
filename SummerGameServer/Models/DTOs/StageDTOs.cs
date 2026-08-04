using SummerGameServer.Models.VOs;
using SummerGameServer.Services;

namespace SummerGameServer.Models.DTOs
{
    //나중가면 추가될수 있는 필드들
    //Reward
    public sealed record GetStageResponse(int Width, int Height, bool[] TileDatas, TrapVO[] TrapDatas);
    public sealed record StageEnterResponse()
    {
        public int RunId { get; set; }
        public StageVO StageData { get; set; } = null!;
        public static StageEnterResponse From(int runId, StageVO stage, CatalogManager catalog)
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
