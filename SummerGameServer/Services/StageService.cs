using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Persistence.Entities;
using SummerGameServer.DbContexts;
using SummerGameServer.Entities;
using SummerGameServer.Models;

namespace SummerGameServer.Services
{
    public enum StageError
    {
        None = 0,
        StageNotFound, 
        RunNotFound, 
        NotYourRun, 
        AlreadyCompleted,
    }

    public class StageService
    {
        private readonly UserDbContext _dbContext;
        private readonly CatalogManager _catalog;
        private readonly CharacterService _characterService;
        private readonly CurrencyService _currencyService;
        public StageService(UserDbContext dbContext, CatalogManager catalog,CharacterService characterService,CurrencyService currencyService)
        {
            _dbContext = dbContext;
            _catalog = catalog;
            _characterService = characterService;
            _currencyService = currencyService;
        }

        //테스트용 스테이지 그냥 생으로 가져오기
        public StageData? GetStage(int stageId)
        {
            return _catalog.GetCatalogModel<StageData>(stageId);
        }
        //실제 스테이지 입장 종료 처리 다 하는 거시기 플레이하는거
        public async Task<StageEnterResponse?> EnterAsync(int userId, int stageId)
        {
            StageData? stageData = _catalog.GetCatalogModel<StageData>(stageId);
            if (stageData is null)
                return null;
            StageRun run = new StageRun() { UserId = userId, StageId = stageId };
            _dbContext.StageRuns.Add(run);
            await _dbContext.SaveChangesAsync();

            return StageEnterResponse.From(run.Id, stageData, _catalog);
        }
        public async Task<(StageError error, StageResultResponse? result)> CompleteAsync(int userId, int runId, StageResultRequest req) 
        {
            StageRun? run = await _dbContext.StageRuns.FirstOrDefaultAsync(run => run.Id == runId);
            if (run is null)
                return (StageError.RunNotFound, null);
            else if(run.UserId != userId)
                return (StageError.NotYourRun, null);
            else if(run.Status != StageRunStatus.InProgress)
                return (StageError.AlreadyCompleted, null);

            StageData? stage = _catalog.GetCatalogModel<StageData>(run.StageId);
            if (stage is null)
                return (StageError.StageNotFound, null);
            //여기서 유저가 남은 체력을 기반으로 보상 계산
            const int maxStarCount = 3;//임시 별 3개 가정
            float currentHealth = 30;
            float maxHealth = 100;
            int starCount = (int)(maxHealth / currentHealth);

            long gainExp = 100;
            gainExp = (gainExp * starCount) / maxStarCount;

            long gainGold = 10;
            gainGold = (gainGold * starCount) / maxStarCount;
            await _currencyService.AddAsync(userId, CurrencyType.Gold, gainGold);//무조건 성공
            Dictionary<CurrencyType, long> currencyGained = new() { { CurrencyType.Gold, gainGold } };

            run.Status = StageRunStatus.Completed;
            run.CompletedAt = DateTime.UtcNow;
            run.CurrenciesGained = JsonConvert.SerializeObject(currencyGained, CatalogManager.JsonSettings);
            run.ExpGained = gainExp;
            CharacterResponse? character = await _characterService.AddExpAsync(userId, (int)gainExp);
            CurrenciesResponse response = (await _currencyService.GetOrCreateAllAsync(userId)).response!;
            var result = new StageResultResponse
            {
                StageId = run.StageId,
                ExpGained = gainExp,
                Character = character!,
                AllCurrencies = response,
                GainCurrencies = new CurrenciesResponse { Currencies = currencyGained},
            };
            return (StageError.None, result);
        }
    }
}
