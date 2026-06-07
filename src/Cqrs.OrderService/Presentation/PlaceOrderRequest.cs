using System.ComponentModel.DataAnnotations;

namespace Cqrs.OrderService.Presentation;

public sealed record PlaceOrderRequest(
    [Required] string CustomerId,
    [Required, MinLength(1)] IReadOnlyList<ItemRequest> Items);
