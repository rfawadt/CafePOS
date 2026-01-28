namespace CafePos.Application.Interfaces;

public interface IClock
{
    DateTime UtcNow { get; }
    DateTime LocalNow { get; }
    DateOnly LocalDate { get; }
}
