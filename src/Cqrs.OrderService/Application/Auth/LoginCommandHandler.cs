using Cqrs.OrderService.Application.Abstractions.Security;
using Cqrs.OrderService.Application.Common.Errors;
using FluentResults;
using MediatR;
using Cqrs.OrderService.Presentation.Auth;

namespace Cqrs.OrderService.Application.Auth;

public sealed class LoginCommandHandler(
    IUserReadRepository users,
    ITokenService tokenService,
    IPasswordVerifier passwordVerifier)
    : IRequestHandler<LoginCommand, Result<TokenResponse>>
{
    public async Task<Result<TokenResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await users.FindByUsername(request.Username, cancellationToken);
        if (user is null || !user.Enabled || !passwordVerifier.Verify(user.Username, request.Password, user.PasswordHash))
        {
            return Result.Fail<TokenResponse>(ApplicationErrors.Unauthorized("Invalid username or password"));
        }

        return Result.Ok(new TokenResponse(tokenService.GenerateToken(user)));
    }
}
