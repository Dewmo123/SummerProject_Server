namespace SummerGameServer.Models.DAOs
{
    //MapId로 관리하기 때문에 Map이 바뀌게되면 합정이랑 겹칠수도
    //나중에 예외처리 해줄게오
    public class UserRoom
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int MapId { get; set; }
        public string TrapData { get; set; } = null!;
    }
}
