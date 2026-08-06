using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SummerGameServer.Extensions;
using SummerGameServer.Models.DTOs;
using SummerGameServer.Services;

namespace SummerGameServer.Controllers;

[Authorize]
[ApiController]
[Route("api/user-room")]
public sealed class UserRoomController(UserRoomService userRoomService) : ControllerBase
{
    [HttpPost("upload")]
    [RequestSizeLimit(64 * 1024)]
    public async Task<ActionResult<UserRoomResponse>> UpsertUserRoom(
        UploadUserRoomRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out int userId))
            return Unauthorized();

        (UserRoomError error, UserRoomResponse? response) = await userRoomService.UpsertAsync(
            userId,
            request,
            cancellationToken);
        return ToActionResult(error, response);
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserRoomResponse>> GetMyRoom(CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out int userId))
            return Unauthorized();

        (UserRoomError error, UserRoomResponse? response) = await userRoomService.GetByUserIdAsync(
            userId,
            cancellationToken);
        return ToActionResult(error, response);
    }

    private ActionResult<UserRoomResponse> ToActionResult(UserRoomError error, UserRoomResponse? response)
    {
        return error switch
        {
            UserRoomError.None => Ok(response),
            UserRoomError.UserNotFound => Unauthorized(),
            UserRoomError.MapNotFound => BadRequest(new { message = "존재하지 않는 맵입니다." }),
            UserRoomError.RoomNotFound => NotFound("룸이 아직 생성되지 않았습니다."),
            UserRoomError.InvalidRoomMap => NotFound("잘못된 MapId입니다. 맵을 다시 업로드해주세요"),
            UserRoomError.UnsupportedTrapType => BadRequest(new { message = "지원하지 않는 함정 타입입니다." }),
            UserRoomError.TrapOutOfBounds => BadRequest(new { message = "함정 좌표가 맵 범위를 벗어났습니다." }),
            UserRoomError.DuplicateTrapPosition => BadRequest(new { message = "같은 위치에 함정을 중복 배치할 수 없습니다." }),
            UserRoomError.InvalidTrapRotation => BadRequest(new { message = "함정 회전값은 정규화된 quaternion이어야 합니다." }),
            _ => StatusCode(500)
        };
    }
}
