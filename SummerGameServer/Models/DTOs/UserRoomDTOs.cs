using SummerGameServer.Models.VOs;

namespace SummerGameServer.Models.DTOs
{
    public sealed record UploadUserRoomRequest
    {
        [System.ComponentModel.DataAnnotations.Range(1, int.MaxValue)]
        public int MapId { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(100)]
        public TrapVO[] TrapDatas { get; set; } = [];
    }

}
