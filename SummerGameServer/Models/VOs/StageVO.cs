namespace SummerGameServer.Models.VOs
{
    public sealed record StageVO : ICatalogModel
    {
        public int Id => StageId;
        public int StageId { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool[] TileDatas { get; set; } = null!;
        public TrapVO[] TrapDatas { get; set; } = null!;
        public int MinimumClearSeconds { get; set; } = 1;
        public long RewardExp { get; set; }
        public long RewardGold { get; set; }
    }
}
