using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SummerGameServer.Extensions;
using SummerGameServer.Models.Entities;
using SummerGameServer.Models.DTOs;
using SummerGameServer.Services;

namespace SummerGameServer.Controllers;

[Authorize]
[ApiController]
[Route("api/currency")]
public sealed class CurrencyController(CurrencyService currencyService) : ControllerBase
{
    [HttpGet("me/{type:int}")]
    public async Task<ActionResult<CurrencyResponse>> GetMyCurrency(CurrencyType type,CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out int userId))
            return Unauthorized();

        (CurrencyError error, CurrencyResponse? response) = await currencyService.GetByUserIdAsync(userId, type, cancellationToken);

        return error switch
        {
            CurrencyError.None => Ok(response),
            CurrencyError.UserNotFound => Unauthorized(),
            CurrencyError.InvalidCurrency => BadRequest(new { message = "올바르지 않은 재화 타입입니다." }),
            _ => StatusCode(500)
        };
    }

    [HttpGet("me")]
    public async Task<ActionResult<CurrenciesResponse>> GetAllMyCurrencies(CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out int userId))
            return Unauthorized();

        (CurrencyError error, CurrenciesResponse? response) = await currencyService.GetOrCreateAllAsync(userId,cancellationToken);

        return error switch
        {
            CurrencyError.None => Ok(response),
            CurrencyError.UserNotFound => Unauthorized(),
            _ => StatusCode(500)
        };
    }
}
