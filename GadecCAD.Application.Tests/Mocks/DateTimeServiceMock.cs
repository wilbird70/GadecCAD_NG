using GadecCAD.Application.Interfaces;

namespace GadecCAD.Application.Tests.Mocks;
internal class DateTimeServiceMock : IDateTimeService
{
    public DateTime Now { get; set; }
    public DateOnly Today { get; set; }
    public DateTime UtcNow { get; set; }
}
