namespace CafePos.Application.Models;

public record PaymentInput(Guid PaymentMethodId, decimal Amount, string? Reference);
