using Newtonsoft.Json;
using SummerGameServer.Models;

namespace SummerGameServer.Services
{
    public class StageService
    {
        public RoomData GetStage(int stageId)
        {
            if (stageId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(stageId),"Stage ID는 1 이상이어야 합니다.");
            }

            string filePath = Path.Combine(AppContext.BaseDirectory,"GameData","Stages",$"Stage{stageId}.json");

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Stage JSON 파일을 찾을 수 없습니다: Stage{stageId}.json",filePath);
            }

            try
            {
                string json = File.ReadAllText(filePath);
                RoomData? roomData = JsonConvert.DeserializeObject<RoomData>(json);
                if (roomData == null)
                    throw new NullReferenceException($"Stage{stageId}.json에서 RoomData를 읽을 수 없습니다.");

                return roomData;
            }
            catch (JsonException exception)
            {
                throw new InvalidDataException($"Stage{stageId}.json 형식이 올바르지 않습니다.",exception);
            }
        }
        public MapData GetMap(int mapId)
        {
            if (mapId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(mapId), "Map ID는 1 이상이어야 합니다.");
            }

            string filePath = Path.Combine(AppContext.BaseDirectory, "GameData", "Maps", $"Map{mapId}.json");

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Map JSON 파일을 찾을 수 없습니다: Map{mapId}.json", filePath);
            }

            try
            {
                string json = File.ReadAllText(filePath);
                MapData? mapData = JsonConvert.DeserializeObject<MapData>(json);
                if (mapData == null)
                    throw new NullReferenceException($"Map{mapId}.json에서 MapData를 읽을 수 없습니다.");

                return mapData;
            }
            catch (JsonException exception)
            {
                throw new InvalidDataException($"Map{mapId}.json 형식이 올바르지 않습니다.", exception);
            }
        }
    }
}
