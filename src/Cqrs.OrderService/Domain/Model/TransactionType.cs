namespace Cqrs.OrderService.Domain.Model;

public enum TransactionType
{
    RECEIVE,
    SHIP,
    RESERVE,
    RELEASE,
    ADJUST
}
