namespace GadecCAD.Application.Interfaces;

public interface IDateTimeService
{
    DateTime Now { get; }
    DateTime UtcNow { get; }
    DateOnly Today { get; }
}