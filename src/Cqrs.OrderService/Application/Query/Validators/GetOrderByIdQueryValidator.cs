using FluentValidation;

namespace Cqrs.OrderService.Application.Query.Validators;

public sealed class GetOrderByIdQueryValidator : AbstractValidator<GetOrderByIdQuery>
{
    public GetOrderByIdQueryValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
    }
}
