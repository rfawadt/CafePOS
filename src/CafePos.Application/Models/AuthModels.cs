namespace CafePos.Application.Models;

public record LoginResult(bool Success, string Message, Guid? UserId, string? DisplayName, IReadOnlyList<string> Roles);
