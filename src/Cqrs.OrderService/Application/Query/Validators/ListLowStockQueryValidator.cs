using FluentValidation;

namespace Cqrs.OrderService.Application.Query.Validators;

public sealed class ListLowStockQueryValidator : AbstractValidator<ListLowStockQuery>
{
    public ListLowStockQueryValidator()
    {
        RuleFor(x => x.Threshold).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Limit).InclusiveBetween(1, 500);
    }
}
