namespace SummerGameServer.Models.DAOs
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

        public long ExpGained;
        public string? CurrenciesGained { get; set; } 
    }
}
