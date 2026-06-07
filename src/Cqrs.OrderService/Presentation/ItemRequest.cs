using System.ComponentModel.DataAnnotations;

namespace Cqrs.OrderService.Presentation;

public sealed record ItemRequest(
    [Required] string ProductId,
    [Required] string ProductName,
    [Range(1, int.MaxValue)] int Quantity,
    [Range(0.01, double.MaxValue)] decimal UnitPrice,
    [Required] string Currency);
