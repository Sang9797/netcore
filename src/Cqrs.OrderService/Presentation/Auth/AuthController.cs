using Cqrs.OrderService.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cqrs.OrderService.Presentation.Auth;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(UserRepository users, JwtTokenService jwt) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<TokenResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var user = await users.FindByUsername(request.Username, cancellationToken);
        if (user is null || !user.Enabled || !PasswordVerifier.Verify(user.Username, request.Password, user.PasswordHash))
        {
            return Unauthorized();
        }

        return Ok(new TokenResponse(jwt.GenerateToken(user)));
    }
}
