using Microsoft.AspNetCore.Mvc;
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
            RoomData roomData = _stageService.GetStage(stageId);
            MapData mapData = _stageService.GetMap(roomData.MapId);

            return new GetStageResponse(mapData.Width, mapData.Height, mapData.TileDatas, roomData.Trapdatas);
        }
        [HttpGet]
        public void GetRandomUserStage()//추후
        {

        }
    }
}
