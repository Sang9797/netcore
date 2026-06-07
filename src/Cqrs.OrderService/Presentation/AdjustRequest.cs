using System.ComponentModel.DataAnnotations;

namespace Cqrs.OrderService.Presentation;

public sealed record AdjustRequest(
    [Required] string ProductId,
    [Required] string WarehouseId,
    int Delta,
    [Required] string Reason);
