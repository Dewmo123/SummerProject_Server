using SummerGameServer.Models.Datas;
using SummerGameServer.Services;

namespace SummerGameServer.Tests;

public sealed class CatalogManagerTests
{
    [Fact]
    public void LoadFrom_LoadsValidatedGameData()
    {
        CatalogManager catalog = CatalogManager.LoadFrom(AppContext.BaseDirectory);

        StageData stage = Assert.IsType<StageData>(catalog.GetCatalogModel<StageData>(1));
        MapData map = Assert.IsType<MapData>(catalog.GetCatalogModel<MapData>(1));

        Assert.Equal(stage.Width * stage.Height, stage.TileDatas.Length);
        Assert.Equal(map.Width * map.Height, map.TileDatas.Length);
        Assert.True(stage.MinimumClearSeconds > 0);
        Assert.True(stage.RewardExp > 0);
        Assert.True(stage.RewardGold > 0);
    }

    [Fact]
    public void LoadFrom_RejectsInvalidTileCount()
    {
        string root = Path.Combine(Path.GetTempPath(), $"summer-catalog-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Path.Combine(root, "GameData", "Maps"));
        Directory.CreateDirectory(Path.Combine(root, "GameData", "Stages"));
        try
        {
            File.WriteAllText(
                Path.Combine(root, "GameData", "Maps", "Map1.json"),
                """{"mapId":1,"width":2,"height":2,"tileDatas":[true]}""");
            File.WriteAllText(
                Path.Combine(root, "GameData", "Stages", "Stage1.json"),
                """{"stageId":1,"width":1,"height":1,"tileDatas":[true],"trapDatas":[],"minimumClearSeconds":1,"rewardExp":1,"rewardGold":1}""");

            Assert.Throws<InvalidDataException>(() => CatalogManager.LoadFrom(root));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
