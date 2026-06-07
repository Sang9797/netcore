using System.ComponentModel.DataAnnotations;

namespace Cqrs.OrderService.Presentation.Auth;

public sealed record LoginRequest([Required] string Username, [Required] string Password);
