using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SummerGameServer.DbContexts;
using SummerGameServer.Extensions;
using SummerGameServer.Models.DTOs;
using SummerGameServer.Models.VOs;
using SummerGameServer.Services;

namespace SummerGameServer.Controllers;

[Authorize]
[ApiController]
[Route("api/user-room")]
public sealed class UserRoomController(UserDbContext dbContext, CatalogManager catalog) : ControllerBase
{
    [HttpPost]
    [RequestSizeLimit(64 * 1024)]
    public async Task<IActionResult> UpsertUserRoom(
        UploadUserRoomRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out int userId))
            return Unauthorized();

        MapVO? map = catalog.GetCatalogModel<MapVO>(request.MapId);
        if (map is null)
            return BadRequest(new { message = "존재하지 않는 맵입니다." });

        string? validationError = ValidateTraps(request.TrapDatas, map);
        if (validationError is not null)
            return BadRequest(new { message = validationError });

        if (!await dbContext.Users.AnyAsync(user => user.Id == userId, cancellationToken))
            return Unauthorized();

        string trapData = JsonConvert.SerializeObject(request.TrapDatas, CatalogManager.JsonSettings);
        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO `UserRooms` (`UserId`, `MapId`, `TrapData`) VALUES ({userId}, {request.MapId}, {trapData}) ON DUPLICATE KEY UPDATE `MapId` = VALUES(`MapId`), `TrapData` = VALUES(`TrapData`)",
            cancellationToken);

        return NoContent();
    }

    private static string? ValidateTraps(IReadOnlyCollection<TrapVO> traps, MapVO map)
    {
        HashSet<(int x, int y, int z)> occupied = [];
        foreach (TrapVO trap in traps)
        {
            if (!Enum.IsDefined(trap.Type))
                return "지원하지 않는 함정 타입입니다.";
            if (trap.Position.x < 0 || trap.Position.x >= map.Width ||
                trap.Position.y < 0 || trap.Position.y >= map.Height ||
                trap.Position.z != 0)
                return "함정 좌표가 맵 범위를 벗어났습니다.";
            if (!occupied.Add((trap.Position.x, trap.Position.y, trap.Position.z)))
                return "같은 위치에 함정을 중복 배치할 수 없습니다.";

            double magnitudeSquared =
                trap.Rotation.x * trap.Rotation.x +
                trap.Rotation.y * trap.Rotation.y +
                trap.Rotation.z * trap.Rotation.z +
                trap.Rotation.w * trap.Rotation.w;
            if (magnitudeSquared is < 0.98 or > 1.02)
                return "함정 회전값은 정규화된 quaternion이어야 합니다.";
        }

        return null;
    }
}
