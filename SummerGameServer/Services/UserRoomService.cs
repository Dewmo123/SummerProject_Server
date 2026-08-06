using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SummerGameServer.DbContexts;
using SummerGameServer.Models.Datas;
using SummerGameServer.Models.DTOs;
using SummerGameServer.Models.Entities;

namespace SummerGameServer.Services;

public enum UserRoomError
{
    None = 0,
    UserNotFound,
    MapNotFound,
    RoomNotFound,
    InvalidRoomMap,
    UnsupportedTrapType,
    TrapOutOfBounds,
    DuplicateTrapPosition,
    InvalidTrapRotation
}

public sealed class UserRoomService(UserDbContext dbContext, CatalogManager catalog)
{
    public async Task<(UserRoomError error, UserRoomResponse? response)> UpsertAsync(
        int userId,
        UploadUserRoomRequest request,
        CancellationToken cancellationToken = default)
    {
        MapData? map = catalog.GetCatalogModel<MapData>(request.MapId);
        if (map is null)
            return (UserRoomError.MapNotFound, null);

        UserRoomError validationError = ValidateTraps(request.TrapDatas, map);
        if (validationError != UserRoomError.None)
            return (validationError, null);

        if (!await dbContext.Users.AnyAsync(user => user.Id == userId, cancellationToken))
            return (UserRoomError.UserNotFound, null);

        string trapData = JsonConvert.SerializeObject(request.TrapDatas, CatalogManager.JsonSettings);
        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO `UserRooms` (`UserId`, `MapId`, `TrapData`) VALUES ({userId}, {request.MapId}, {trapData}) ON DUPLICATE KEY UPDATE `MapId` = VALUES(`MapId`), `TrapData` = VALUES(`TrapData`)",
            cancellationToken);

        return (UserRoomError.None, new UserRoomResponse
        {
            MapData = map,
            TrapDatas = request.TrapDatas,
            UserId = userId
        });
    }

    public async Task<(UserRoomError error, UserRoomResponse? response)> GetByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        UserRoom? room = await dbContext.UserRooms
            .AsNoTracking()
            .SingleOrDefaultAsync(room => room.UserId == userId, cancellationToken);
        if (room is null)
            return (UserRoomError.RoomNotFound, null);

        MapData? map = catalog.GetCatalogModel<MapData>(room.MapId);
        if (map is null)
            return (UserRoomError.InvalidRoomMap, null);

        TrapData[] trapDatas = JsonConvert.DeserializeObject<TrapData[]>(room.TrapData) ?? [];
        return (UserRoomError.None, new UserRoomResponse
        {
            MapData = map,
            TrapDatas = trapDatas,
            UserId = userId
        });
    }

    private static UserRoomError ValidateTraps(IReadOnlyCollection<TrapData> traps, MapData map)
    {
        HashSet<(int x, int y, int z)> occupied = [];
        foreach (TrapData trap in traps)
        {
            if (!Enum.IsDefined(trap.Type))
                return UserRoomError.UnsupportedTrapType;
            if (trap.Position.x < 0 || trap.Position.x >= map.Width ||
                trap.Position.y < 0 || trap.Position.y >= map.Height ||
                trap.Position.z != 0)
                return UserRoomError.TrapOutOfBounds;
            if (!occupied.Add((trap.Position.x, trap.Position.y, trap.Position.z)))
                return UserRoomError.DuplicateTrapPosition;

            double magnitudeSquared =
                trap.Rotation.x * trap.Rotation.x +
                trap.Rotation.y * trap.Rotation.y +
                trap.Rotation.z * trap.Rotation.z +
                trap.Rotation.w * trap.Rotation.w;
            if (magnitudeSquared is < 0.98 or > 1.02)
                return UserRoomError.InvalidTrapRotation;
        }

        return UserRoomError.None;
    }
}
