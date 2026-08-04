using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SummerGameServer.Extensions;
using SummerGameServer.Models.DAOs;
using SummerGameServer.Models.DTOs;
using SummerGameServer.Services;

namespace SummerGameServer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/currency")]
    public class CurrencyController : ControllerBase
    {
        private readonly CurrencyService _currencyService;
        public CurrencyController(CurrencyService currency)
        {
            _currencyService = currency;
        }
        [HttpGet("me/{type:int}")]
        public async Task<ActionResult<CurrencyResponse>> GetMyCurrency(CurrencyType type)
        {
            if (!User.TryGetUserId(out int userId))
                return Unauthorized();
            (CurrencyError error, CurrencyResponse? res) = await _currencyService.GetByUserIdAsync(userId, type);
            return error switch
            {
                CurrencyError.None => Ok(res),
                CurrencyError.UserNotFound => NotFound(new { message = "유저를 찾을수 없습니다." }),
                CurrencyError.InvalidCurrency => NotFound(new { message = "재화 타입을 찾을수 없습니다." }),
                _ => StatusCode(500)
            };
        }
        [HttpGet("me")]
        public async Task<ActionResult<CurrenciesResponse>> GetAllMyCurrencies()
        {
            if (!User.TryGetUserId(out int userId))
                return Unauthorized();

            (CurrencyError error, CurrenciesResponse? currency) = await _currencyService.GetOrCreateAllAsync(userId);
            if (error == CurrencyError.UserNotFound || currency == null)
                return NotFound();
            return Ok(currency);
        }
    }
}
