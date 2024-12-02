namespace SurveyBasket.Controllers;

[Route("me")]
[ApiController]
[Authorize]
public class AccountController(IUserService userService) : ControllerBase
{
    private readonly IUserService userService = userService;

    [HttpGet("")]
    public async Task<IActionResult> Info()
    {
        var result = await userService.GetProfileAsync(User.GetUserId()!);

        return Ok(result.Value);
    }
}
