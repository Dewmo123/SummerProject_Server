using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SummerGameServer.Extensions;
using SummerGameServer.Models.DTOs;
using SummerGameServer.Services;

namespace SummerGameServer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/character")]
    public class CharacterController : ControllerBase
    {
        private readonly CharacterService _characterService;
        public CharacterController(CharacterService characterService)
        {
            _characterService = characterService;
        }
        [HttpGet("me")]
        public async Task<ActionResult<CharacterResponse>> Me()
        {
            if (!User.TryGetUserId(out int userId))
                return Unauthorized();
            var character = await _characterService.GetByUserIdAsync(userId);
            if (character is null)
                return NotFound(new { message = "캐릭터를 찾을 수 없습니다." });
            return Ok(character);
        }
        [HttpPost("me/gain-exp")]
        public async Task<ActionResult<CharacterResponse>> GainExp(GainExpRequest request)
        {
            if (!User.TryGetUserId(out int userId))
                return Unauthorized();
            var character = await _characterService.AddExpAsync(userId, request.Amount);

            if (character is null)
                return NotFound(new { message = "캐릭터를 찾을 수 없습니다." });
            return Ok(character);
        }
    }
}
