using SummerGameServer.Models.VOs;

namespace SummerGameServer.Models.DTOs
{
    public sealed record UploadUserRoomRequest {
        public int MapId { get; set; }
        public TrapVO[] TrapDatas { get; set; } = null!;
    }

}
