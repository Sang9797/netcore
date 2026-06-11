using FluentValidation;

namespace Cqrs.OrderService.Application.Command.Validators;

public sealed class ReleaseInventoryCommandValidator : AbstractValidator<ReleaseInventoryCommand>
{
    public ReleaseInventoryCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
