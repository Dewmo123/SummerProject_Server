namespace SummerGameServer.Models.Entities
{
    public class UserRoom
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public Persistence.Entities.User User { get; set; } = null!;
        public int MapId { get; set; }
        public string TrapData { get; set; } = null!;
    }
}
