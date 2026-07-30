using SummerGameServer.Entities;
using SummerGameServer.Services;

namespace SummerGameServer.Models
{
    //나중가면 추가될수 있는 필드들
    //Reward
    public sealed record StageData(int StageId, int Width, int Height, bool[] TileDatas, TrapData[] Trapdatas) : ICatalogModel
    {
        public int Id => StageId;
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

    }
}
