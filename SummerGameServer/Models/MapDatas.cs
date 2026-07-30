
namespace SummerGameServer.Models
{
    public sealed record RoomData(int MapId, TrapData[] Trapdatas);
    public sealed record MapData(int MapId, int Width,int Height, bool[] TileDatas = null!);
    public sealed record TrapData(TrapType Type, Vector3Int Position, Quaternion Rotation);
    public enum TrapType
    {
        SawTrap,
    }
    public sealed record GetStageResponse(int Width, int Height, bool[] TileDatas = null!, TrapData[] TrapDatas = null!);

    public struct Vector3Int
    {
        public int x { get; set; }
        public int y { get; set; }
        public int z { get; set; }
    }
    public struct Quaternion
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public float w { get; set; }
    }
}
