using Autodesk.AutoCAD.ApplicationServices;
using GadecCAD.Helpers;
using GadecCAD.Models;
using GadecLibrary.Helpers;

namespace GadecCAD.Services;
public class FrameSetService
{
    public FrameData FrameData { get; private set; }

    private string _fileName;
    private string _folder;
    private bool _justSaved;
    private Dictionary<string, Document> _documents;
    private bool _folderHasWritePermission;

    FrameSetService(string fileName, bool justSaved)
    {
        _fileName = fileName;
        _folder = Path.GetDirectoryName(fileName) ?? throw new ArgumentException("Not able to parse path", nameof(fileName));
        _justSaved = justSaved;
        _documents = DocumentsHelper.GetOpenDocuments();
        _folderHasWritePermission = FileSystemHelper.FolderHasWritePermission(_folder);

        try
        {

        }
        catch
        {
        }
    }



}
