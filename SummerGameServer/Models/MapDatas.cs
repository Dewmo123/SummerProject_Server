using System.Numerics;

namespace SummerGameServer.Models
{
    public sealed record RoomData(int MapId, TrapData[] Trapdatas);
    public sealed record MapData(int MapId, int Width,int Height, bool[] TileDatas = null!);
    public sealed record TrapData(TrapType TrapType, Vector3 Position, Quaternion Rotation);
    public enum TrapType
    {
        SawTrap,
    }
    public sealed record GetStageResponse(int Width, int Height, bool[] TileDatas = null!, TrapData[] TrapDatas = null!);
}
