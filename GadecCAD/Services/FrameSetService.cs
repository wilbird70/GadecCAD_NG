using Autodesk.AutoCAD.ApplicationServices;
using GadecCAD.Constants;
using GadecCAD.Helpers;
using GadecCAD.Models;
using GadecLibrary.Helpers;

namespace GadecCAD.Services;
public class FrameSetService
{
    public List<FrameData> UpdatedFrameListData { get; } = [];

    private readonly XmlService _xml;

    private DrawingList _drawingList = new();
    private string _fileName = string.Empty;
    private string _folder = string.Empty;
    private bool _justSaved;
    private Dictionary<string, Document> _documents = [];
    private bool _folderHasWritePermission = true;

    public FrameSetService(XmlService xmlConverter)
    {
        _xml = Guard.ForNull(xmlConverter);
    }

    public bool UpdateDrawingList(string dwgFileName, bool justSaved = false)
    {
        _fileName = dwgFileName;
        _folder = Path.GetDirectoryName(dwgFileName) ?? throw new ArgumentException("Not able to parse path", nameof(dwgFileName));
        _justSaved = justSaved;
        _documents = DocumentsHelper.GetOpenDocuments();
        _folderHasWritePermission = FileSystemHelper.FolderHasWritePermission(_folder);

        try
        {
            var xmlFileName = Path.Combine(_folder, "Drawinglist.xml");
            _drawingList = _xml.Read<DrawingList>(xmlFileName);




            _xml.Write(_drawingList, Path.Combine(_folder, "Drawinglist_copy.xml"));
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
