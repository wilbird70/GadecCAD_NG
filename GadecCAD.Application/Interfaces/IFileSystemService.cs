namespace GadecCAD.Application.Interfaces;
public interface IFileSystemService
{
    List<(string DwgFile, DateTime DwgDate)> GetDrawingFiles(string folder);
    bool FolderHasWritePermission(string folderPath);
}