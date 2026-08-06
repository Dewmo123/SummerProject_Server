using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SummerGameServer.Extensions;
using SummerGameServer.Models.DTOs;
using SummerGameServer.Models.Datas;
using SummerGameServer.Services;

namespace SummerGameServer.Controllers;

[ApiController]
[Authorize]
[Route("api/stage")]
public sealed class StageController(StageService stageService) : ControllerBase
{
    [HttpGet("{stageId:int}")]
    [AllowAnonymous]
    public ActionResult<GetStageResponse> GetStaticStage(int stageId)
    {
        StageData? stageData = stageService.GetStage(stageId);
        return stageData is null
            ? NotFound(new { message = "존재하지 않는 스테이지입니다." })
            : Ok(new GetStageResponse(
                stageData.Width,
                stageData.Height,
                stageData.TileDatas,
                stageData.TrapDatas));
    }

    [HttpPost("{stageId:int}/enter")]
    public async Task<ActionResult<StageEnterResponse>> Enter(int stageId,CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out int userId))
            return Unauthorized();

        (StageError error, StageEnterResponse? response) = await stageService.EnterAsync(
            userId,
            stageId,
            cancellationToken);
        return error switch
        {
            StageError.None => Ok(response),
            StageError.StageNotFound => NotFound(new { message = "존재하지 않는 스테이지입니다." }),
            StageError.UserNotFound => Unauthorized(),
            _ => StatusCode(500)
        };
    }

    [HttpPost("runs/{runId:int}/complete")]
    public async Task<ActionResult<StageResultResponse>> Complete(int runId,CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out int userId))
            return Unauthorized();

        (StageError error, StageResultResponse? response) = await stageService.CompleteAsync(
            userId,
            runId,
            cancellationToken);
        return error switch
        {
            StageError.None => Ok(response),
            StageError.StageNotFound => NotFound(new { message = "존재하지 않는 던전입니다." }),
            StageError.RunNotFound => NotFound(new { message = "던전 기록을 찾을 수 없습니다." }),
            StageError.NotYourRun => Forbid(),
            StageError.AlreadyCompleted => Conflict(new { message = "이미 처리된 플레이 기록입니다." }),
            StageError.TooEarly => UnprocessableEntity(new { message = "최소 클리어 시간이 지나지 않았습니다." }),
            StageError.UserNotFound => Unauthorized(),
            StageError.RewardFailed => StatusCode(500, new { message = "보상 지급에 실패했습니다." }),
            _ => StatusCode(500)
        };
    }
}
