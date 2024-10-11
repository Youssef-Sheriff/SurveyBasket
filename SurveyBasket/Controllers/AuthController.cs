using Microsoft.Extensions.Options;
using SurveyBasket.Authentication;

namespace SurveyBasket.Controllers;

[Route("[controller]")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    private readonly IAuthService _authService = authService;

    [HttpPost("")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginReqest reqest, CancellationToken cancellationToken)
    {
        var authResult = await _authService.GetTokenAsync(reqest.Email, reqest.Password, cancellationToken);

        return authResult.IsSuccess ? Ok(authResult.Value) : BadRequest(authResult.Error);

    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshAsync([FromBody] RefreshTokenRequest reqest, CancellationToken cancellationToken)
    {
        var authResult = await _authService.GetRefreshTokenAsync(reqest.Token, reqest.RefreshToken, cancellationToken);

        return authResult is null ? BadRequest("Invalid token") : Ok(authResult);
    }

    [HttpPut("revoke-refresh-token")]
    public async Task<IActionResult> RevokeRefreshTokenAsync([FromBody] RefreshTokenRequest reqest, CancellationToken cancellationToken)
    {
        var isRevoked = await _authService.RevokeRefreshTokenAsync(reqest.Token, reqest.RefreshToken, cancellationToken);

        return isRevoked ? Ok(isRevoked) : BadRequest("Operation faild");
    }

}