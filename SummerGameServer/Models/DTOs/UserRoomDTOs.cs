using SummerGameServer.Models.Datas;

namespace SummerGameServer.Models.DTOs
{
    public sealed record UploadUserRoomRequest
    {
        [System.ComponentModel.DataAnnotations.Range(1, int.MaxValue)]
        public int MapId { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(100)]
        public TrapProto[] TrapDatas { get; set; } = [];
    }
    public sealed record UserRoomResponse
    {
        public int UserId { get; set; }
        public MapProto MapData { get; set; } = null!;
        public TrapProto[] TrapDatas { get; set; } = [];
    }

}
