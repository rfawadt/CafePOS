using CafePos.Application.Interfaces;

namespace CafePos.Infrastructure.Services;

public class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
    public DateTime LocalNow => DateTime.Now;
    public DateOnly LocalDate => DateOnly.FromDateTime(LocalNow);
}
