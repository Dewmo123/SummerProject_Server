namespace SummerGameServer.Models.DAOs
{
    public enum StageRunStatus
    {
        InProgress = 0,
        Completed = 1,
        Abandoned = 2,
    }

    public class StageRun
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public Persistence.Entities.User User { get; set; } = null!;
        public int StageId { get; set; }
        public StageRunStatus Status { get; set; } = StageRunStatus.InProgress;
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        public long ExpGained { get; set; }
        public string? CurrenciesGained { get; set; }
    }
}
