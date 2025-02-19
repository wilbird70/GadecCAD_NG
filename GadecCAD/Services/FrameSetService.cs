using Autodesk.AutoCAD.ApplicationServices;
using GadecCAD.Constants;
using GadecCAD.Helpers;
using GadecCAD.Models;
using GadecLibrary.Helpers;

namespace GadecCAD.Services;
public class FrameSetService
{
    public List<FrameData> UpdatedFrameListData { get; } = [];
    private DrawingList _drawingList = new();

    private string _fileName = string.Empty;
    private string _folder = string.Empty;
    private bool _justSaved;
    private Dictionary<string, Document> _documents = [];
    private bool _folderHasWritePermission = true;

    public bool UpdateDrawingList(string fileName, bool justSaved)
    {
        _fileName = fileName;
        _folder = Path.GetDirectoryName(fileName) ?? throw new ArgumentException("Not able to parse path", nameof(fileName));
        _justSaved = justSaved;
        _documents = DocumentsHelper.GetOpenDocuments();
        _folderHasWritePermission = FileSystemHelper.FolderHasWritePermission(_folder);

        try
        {
            var xmlFileName = Path.Combine(_folder, "Drawinglist.xml");
            _drawingList = XmlConverter.Read<DrawingList>(xmlFileName);




            XmlConverter.Write(_drawingList, xmlFileName);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void CompareLastWriteTimesAndCopyUnmodified()
    {
        var dwgFiles = Directory.GetFiles(_folder, SearchPatternConstants.Drawings);

        foreach (var dwgFile in dwgFiles)
        {
            var currentFrames = _drawingList.Frames.Where(e => e.Filename == dwgFile);
            var currentFiles = _drawingList.Files.Where(e => e.Filename == dwgFile);






        }
    }

}
