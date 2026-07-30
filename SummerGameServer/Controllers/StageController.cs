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
            StageData stageData = _stageService.GetStage(stageId);

            return new GetStageResponse(stageData.Width,stageData.Height,stageData.TileDatas,stageData.Trapdatas);
        }
        [HttpPost("{stageId:int}/enter")]
        public async Task<IActionResult> Enter(int stageId)
        {
            if (!User.TryGetUserId(out int userId))
                return Unauthorized();

            StageEnterResponse? response = await _stageService.EnterAsync(userId, stageId);
            if (response is null)
                return NotFound(new { Message = "존재하지 않는 스테이지입니다." });
            return Ok(response);
        }
    }
}
