namespace SummerGameServer.Models.Datas
{
    public sealed record MapProto(int MapId, int Width, int Height, bool[] TileDatas) : ICatalogModel
    {
        public int Id => MapId;
    }

    public enum TrapType
    {
        SawTrap,
    }
    public struct Vector3IntProto
    {
        public int x { get; set; }
        public int y { get; set; }
        public int z { get; set; }
    }
    public struct QuaternionProto
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public float w { get; set; }
    }
}
