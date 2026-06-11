using FluentValidation;

namespace Cqrs.OrderService.Application.Command.Validators;

public sealed class AdjustInventoryCommandValidator : AbstractValidator<AdjustInventoryCommand>
{
    public AdjustInventoryCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty();
        RuleFor(x => x.Delta).NotEqual(0);
    }
}
