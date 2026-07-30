namespace SummerGameServer.Entities
{
    public enum StageRunStatus
    {
        InProgress = 0,
        Completed = 1,
    }

    public class StageRun
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int StageId { get; set; }
        public StageRunStatus Status{ get; set; } = StageRunStatus.InProgress;
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        //정산 결과는 나중에 더 추가될수 잇음
        public long GoldGained;

        public long ExpGained;
    }
}
