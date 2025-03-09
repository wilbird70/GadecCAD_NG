using GadecCAD.Application.Interfaces;

namespace GadecCAD.Infrastructure.FileSystemServices;
public class FileService : IFileService
{
    private const string DrawingSearchPattern = "*.dwg";

    public string[] GetDrawingFiles(string folder)
    {
        return Directory.GetFiles(folder, DrawingSearchPattern);
    }
}
