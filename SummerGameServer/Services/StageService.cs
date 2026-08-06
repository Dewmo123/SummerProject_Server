using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Persistence.Entities;
using SummerGameServer.DbContexts;
using SummerGameServer.Models.Entities;
using SummerGameServer.Models.DTOs;
using SummerGameServer.Models.Datas;
using System.Data;

namespace SummerGameServer.Services;

public enum StageError
{
    None = 0,
    StageNotFound,
    RunNotFound,
    NotYourRun,
    AlreadyCompleted,
    TooEarly,
    UserNotFound,
    RewardFailed
}

public sealed class StageService(UserDbContext dbContext, CatalogManager catalog, CharacterService characterService, CurrencyService currencyService)
{
    public StageData? GetStage(int stageId) => catalog.GetCatalogModel<StageData>(stageId);

    public async Task<(StageError error, StageEnterResponse? response)> EnterAsync(int userId, int stageId, CancellationToken cancellationToken = default)
    {
        StageData? stageData = catalog.GetCatalogModel<StageData>(stageId);
        if (stageData is null)
            return (StageError.StageNotFound, null);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted,
            cancellationToken);

        User? user = await dbContext.Users
            .FromSqlInterpolated($"SELECT * FROM `Users` WHERE `Id` = {userId} FOR UPDATE")
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);
        if (user is null)
            return (StageError.UserNotFound, null);

        await dbContext.StageRuns
            .Where(run => run.UserId == userId && run.Status == StageRunStatus.InProgress)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(run => run.Status, StageRunStatus.Abandoned)
                    .SetProperty(run => run.CompletedAt, DateTime.UtcNow),
                cancellationToken);

        StageRun run = new() { UserId = userId, StageId = stageId };
        dbContext.StageRuns.Add(run);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return (StageError.None, StageEnterResponse.From(run.Id, stageData,catalog));
    }

    public async Task<(StageError error, StageResultResponse? result)> CompleteAsync(int userId, int runId, CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted,
            cancellationToken);//여러 테이블을 동시성 지켜서 업데이트 하므로 트랜잭션 사용

        StageRun? run = await dbContext.StageRuns
            .AsNoTracking() //엔티티의 상태를 기억하지 않으므로 효율적
            .SingleOrDefaultAsync(candidate => candidate.Id == runId, cancellationToken);
        if (run is null)
            return (StageError.RunNotFound, null);
        if (run.UserId != userId)
            return (StageError.NotYourRun, null);
        if (run.Status != StageRunStatus.InProgress)
            return (StageError.AlreadyCompleted, null);

        StageData? stage = catalog.GetCatalogModel<StageData>(run.StageId);
        if (stage is null)
            return (StageError.StageNotFound, null);

        DateTime completedAt = DateTime.UtcNow;
        if (completedAt - run.StartedAt < TimeSpan.FromSeconds(stage.MinimumClearSeconds))
            return (StageError.TooEarly, null);

        Dictionary<CurrencyType, long> currencyGained = new()
        {
            [CurrencyType.Gold] = stage.RewardGold
        };
        string serializedCurrencies = JsonConvert.SerializeObject(currencyGained, CatalogManager.JsonSettings);

        int claimed = await dbContext.StageRuns
            .Where(candidate =>
                candidate.Id == runId &&
                candidate.UserId == userId &&
                candidate.Status == StageRunStatus.InProgress)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(candidate => candidate.Status, StageRunStatus.Completed)
                    .SetProperty(candidate => candidate.CompletedAt, completedAt)
                    .SetProperty(candidate => candidate.CurrenciesGained, serializedCurrencies)
                    .SetProperty(candidate => candidate.ExpGained, stage.RewardExp),
                cancellationToken);
        if (claimed != 1)
            return (StageError.AlreadyCompleted, null);

        (CurrencyError currencyError, _) = await currencyService.AddAsync(userId, CurrencyType.Gold, stage.RewardGold, cancellationToken);
        if (currencyError != CurrencyError.None)
            return (StageError.RewardFailed, null);

        CharacterResponse? character = await characterService.AddExpAsync(userId, stage.RewardExp, cancellationToken);
        if (character is null)
            return (StageError.UserNotFound, null);

        (CurrencyError allCurrencyError, CurrenciesResponse? allCurrencies) =
            await currencyService.GetOrCreateAllAsync(userId, cancellationToken);
        if (allCurrencyError != CurrencyError.None || allCurrencies is null)
            return (StageError.RewardFailed, null);

        await transaction.CommitAsync(cancellationToken);

        return (StageError.None, new StageResultResponse
        {
            StageId = run.StageId,
            ExpGained = stage.RewardExp,
            Character = character,
            AllCurrencies = allCurrencies,
            GainCurrencies = new CurrenciesResponse { Currencies = currencyGained }
        });
    }
}
