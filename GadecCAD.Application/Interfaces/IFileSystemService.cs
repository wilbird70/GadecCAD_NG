namespace GadecCAD.Application.Interfaces;
public interface IFileSystemService
{
    string[] GetDrawingFiles(string folder);
    DateTime GetLastWriteTimeUtc(string fileName);
    bool FolderHasWritePermission(string folderPath);
}