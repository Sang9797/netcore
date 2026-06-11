using FluentValidation;

namespace Cqrs.OrderService.Application.Query.Validators;

public sealed class ListOrdersByCustomerQueryValidator : AbstractValidator<ListOrdersByCustomerQuery>
{
    public ListOrdersByCustomerQueryValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
    }
}
