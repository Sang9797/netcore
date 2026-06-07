using System.ComponentModel.DataAnnotations;

namespace Cqrs.OrderService.Presentation;

public sealed record CancelRequest([Required] string Reason);
