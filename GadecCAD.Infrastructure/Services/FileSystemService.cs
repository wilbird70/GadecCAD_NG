using GadecCAD.Application.Interfaces;

namespace GadecCAD.Infrastructure.Services;
public class FileSystemService : IFileSystemService
{
    private const string DrawingSearchPattern = "*.dwg";

    public string[] GetDrawingFiles(string folder) => Directory.GetFiles(folder, DrawingSearchPattern);
    public DateTime GetLastWriteTimeUtc(string fileName) => File.GetLastWriteTimeUtc(fileName);

    public bool FolderHasWritePermission(string folderPath)
    {
        try
        {
            string testFile = Path.Combine(folderPath, Path.GetRandomFileName());
            using (File.Create(testFile))
            { }
            File.Delete(testFile);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
