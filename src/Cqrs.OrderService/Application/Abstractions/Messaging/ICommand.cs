using FluentResults;
using MediatR;

namespace Cqrs.OrderService.Application.Abstractions.Messaging;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>;
