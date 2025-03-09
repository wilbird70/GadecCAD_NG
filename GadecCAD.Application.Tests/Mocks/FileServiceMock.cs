using GadecCAD.Application.Interfaces;

namespace GadecCAD.Application.Tests.Mocks;
internal class FileServiceMock : IFileService
{
    public string[] GetDrawingFiles(string folder)
    {
        throw new NotImplementedException();
    }
}
