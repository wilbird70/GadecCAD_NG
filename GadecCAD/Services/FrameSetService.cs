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

    private DrawingList _oldList = new();
    private DrawingList _newList = new();
    private string _fileName = string.Empty;
    private string _folder = string.Empty;
    private bool _justSaved;
    private Dictionary<string, Document> _documents = [];
    private bool _folderHasWritePermission = true;
    private List<string> _filesToReadForNewList = [];
    private List<string> _filesToReadForUpToDateFrames = [];

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
            _oldList = _xml.Read<DrawingList>(xmlFileName);

            CompareLastWriteTimesAndCopyUnmodified();



            _xml.Write(_oldList, Path.Combine(_folder, "Drawinglist_copy.xml"));
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void CompareLastWriteTimesAndCopyUnmodified()
    {
        var dwgFiles = Directory.GetFiles(_folder, SearchPatternConstants.Drawings);

        foreach (var dwgFile in dwgFiles)
        {
            var frames = _oldList.Frames.Where(e => e.Filename == dwgFile);
            var files = _oldList.Files.Where(e => e.Filename == dwgFile);

            if (dwgFile == _fileName && _justSaved)
            {
                _filesToReadForNewList.Add(dwgFile);
                _filesToReadForUpToDateFrames.Add(dwgFile);
                continue;
            }

            if (_documents.ContainsKey(dwgFile))
            {
                if (frames.Any()) { _newList.Frames.AddRange(frames); }
                else if (files.Any()) { _newList.Files.Add(files.First()); }
                _filesToReadForUpToDateFrames.Add(dwgFile);
                continue;
            }

            if (frames.Any())
            {
                if (IsFileDateChanged(frames, File.GetLastWriteTimeUtc(dwgFile)))
                {

                }

            }

            if (files.Any())
            {
                if (IsFileDateChanged(files, File.GetLastWriteTimeUtc(dwgFile)))
                {

                }

            }



        }
    }

    private static bool IsFileDateChanged(IEnumerable<IFileData> data, DateTime lastWriteTimeUtc)
        => data.All(e => e.FileDate == lastWriteTimeUtc);
}
