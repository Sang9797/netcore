using FluentValidation;

namespace Cqrs.OrderService.Application.Query.Validators;

public sealed class GetInventoryReportQueryValidator : AbstractValidator<GetInventoryReportQuery>
{
    public GetInventoryReportQueryValidator()
    {
        RuleFor(x => x.MinStock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Page).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 500);
    }
}
