using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SummerGameServer.DbContexts;
using SummerGameServer.Entities;
using SummerGameServer.Extensions;
using SummerGameServer.Models;
using SummerGameServer.Services;

namespace SummerGameServer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/user-room")]
    public class UserRoomController : ControllerBase
    {
        private UserDbContext _dbContext;
        private StageService _stageService;
        public UserRoomController(UserDbContext dbContext,StageService stageService)
        {
            _dbContext = dbContext;
            _stageService = stageService;
        }
        [HttpPost]
        public async Task<IActionResult> UpsertUserRoom(UploadUserRoomRequest request)
        {
            if (!User.TryGetUserId(out int userId))
                return Unauthorized();
            UserRoom? userRoom = await _dbContext.UserRooms.SingleOrDefaultAsync(userRoom => userRoom.UserId == userId);
            if (userRoom == null)
            {
                userRoom = new UserRoom() { UserId = userId };
                await _dbContext.AddAsync(userRoom);
            }
            userRoom.MapId = request.MapId;
            string trapData = JsonConvert.SerializeObject(request.TrapDatas);
            userRoom.TrapData = trapData;
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
