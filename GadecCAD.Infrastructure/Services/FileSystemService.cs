using GadecCAD.Application.Interfaces;

namespace GadecCAD.Infrastructure.Services;
public class FileSystemService : IFileSystemService
{
    private const string DrawingSearchPattern = "*.dwg";

    public List<(string DwgFile, DateTime DwgDate)> GetDrawingFiles(string folder)
    {
        try
        {
            return Directory.GetFiles(folder, DrawingSearchPattern).Select(e => (e, File.GetLastWriteTimeUtc(e))).ToList();
        }
        catch
        {
            return [];
        }
    }

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
