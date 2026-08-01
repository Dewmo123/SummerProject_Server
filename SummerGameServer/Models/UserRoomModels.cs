namespace SummerGameServer.Models
{
    public sealed record UploadUserRoomRequest {
        public int MapId { get; set; }
        public TrapData[] TrapDatas { get; set; } = null!;
    }

}
