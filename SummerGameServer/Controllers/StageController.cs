using Microsoft.AspNetCore.Mvc;
using SummerGameServer.Extensions;
using SummerGameServer.Models;
using SummerGameServer.Services;

namespace SummerGameServer.Controllers
{
    [ApiController]
    [Route("api/stage")]
    public class StageController : ControllerBase
    {
        private StageService _stageService;
        public StageController(StageService stageService)
        {
            _stageService = stageService;
        }
        [HttpGet("{stageId:int}")]
        public GetStageResponse GetStaticStage(int stageId)
        {
            StageData stageData = _stageService.GetStage(stageId)!;

            return new GetStageResponse(stageData.Width,stageData.Height,stageData.TileDatas,stageData.Trapdatas);
        }
        [HttpPost("{stageId:int}/enter")]
        public async Task<ActionResult<StageEnterResponse>> Enter(int stageId)
        {
            if (!User.TryGetUserId(out int userId))
                return Unauthorized();

            StageEnterResponse? response = await _stageService.EnterAsync(userId, stageId);
            if (response is null)
                return NotFound(new { Message = "존재하지 않는 스테이지입니다." });
            return Ok(response);
        }
        [HttpPost("runs/{runId:int}/complete")]
        public async Task<ActionResult<StageResultResponse>> Complete(int runId,[FromBody]StageResultRequest req)
        {
            if (!User.TryGetUserId(out int userId))
                return Unauthorized();
            (StageError error, StageResultResponse? response) = await _stageService.CompleteAsync(userId, runId, req);
            return error switch
            {
                StageError.None => Ok(response),
                StageError.StageNotFound => NotFound(new { message = "존재하지 않는 던전입니다." }),
                StageError.RunNotFound => NotFound(new { message = "던전 기록을 찾을 수 없습니다." }),
                StageError.NotYourRun => Forbid(),
                StageError.AlreadyCompleted => Conflict(new { message = "이미 정상완료되었습니다." }),
                _=>StatusCode(500)
            };
        }
    }
}
