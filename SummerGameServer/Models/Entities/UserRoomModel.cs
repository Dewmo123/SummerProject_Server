namespace SummerGameServer.Models.Entities
{
    public class UserRoomModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public Persistence.Entities.UserModel User { get; set; } = null!;
        public int MapId { get; set; }
        public string TrapData { get; set; } = null!;
    }
}
