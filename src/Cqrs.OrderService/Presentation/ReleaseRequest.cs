using System.ComponentModel.DataAnnotations;

namespace Cqrs.OrderService.Presentation;

public sealed record ReleaseRequest(
    [Required] string ProductId,
    [Required] string WarehouseId,
    [Range(1, int.MaxValue)] int Quantity,
    [Required] string OrderId);
