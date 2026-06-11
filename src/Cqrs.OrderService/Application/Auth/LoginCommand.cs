using Cqrs.OrderService.Application.Abstractions.Messaging;
using Cqrs.OrderService.Presentation.Auth;

namespace Cqrs.OrderService.Application.Auth;

public sealed record LoginCommand(string Username, string Password) : ICommand<TokenResponse>;
