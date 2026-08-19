namespace SummerGameServer.Models.Datas
{
    public sealed record StageProto : ICatalogModel
    {
        public int Id => StageId;
        public int StageId { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool[] TileDatas { get; set; } = null!;
        public TrapProto[] TrapDatas { get; set; } = null!;
        public int MinimumClearSeconds { get; set; } = 1;
        public long RewardExp { get; set; }
        public long RewardGold { get; set; }
    }
}
