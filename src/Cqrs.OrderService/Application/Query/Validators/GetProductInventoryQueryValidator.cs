using FluentValidation;

namespace Cqrs.OrderService.Application.Query.Validators;

public sealed class GetProductInventoryQueryValidator : AbstractValidator<GetProductInventoryQuery>
{
    public GetProductInventoryQueryValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
    }
}
