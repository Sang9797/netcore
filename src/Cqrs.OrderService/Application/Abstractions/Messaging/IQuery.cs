using FluentResults;
using MediatR;

namespace Cqrs.OrderService.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
