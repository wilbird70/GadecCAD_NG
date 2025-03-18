namespace Gadec.Common.Helpers;
public static class FileSystemHelper
{
    public static bool FolderHasWritePermission(string folderPath)
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
