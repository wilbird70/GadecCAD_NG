using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Gadec.Common.Extensions;
using Gadec.Common.Handlers;
using Gadec.Common.Helpers;
using GadecCAD.Application.Constants;
using GadecCAD.Application.Extensions;
using GadecCAD.Application.Helpers;
using GadecCAD.Application.Models;
using FileToRead = (string File, bool ForDrawingList);

namespace GadecCAD.Application.Services;
public class FrameSetService
{
    public List<FrameData> UpdatedFrameListData { get; } = [];
    public event EventHandler? ProgressChanged;

    private readonly XmlService<DrawingList> _xmlService;
    private readonly FrameInfoService _frameInfoService;

    private DrawingList _oldList = new();
    private readonly DrawingList _newList = new();
    private readonly List<FileToRead> _filesToRead = [];
    private string _fileName = string.Empty;
    private string _folder = string.Empty;
    private bool _justSaved;
    private Dictionary<string, Document> _documents = [];
    private bool _folderHasWritePermission = true;

    public FrameSetService(XmlService<DrawingList> xmlService, FrameInfoService frameInfoService)
    {
        _xmlService = Guard.ForNull(xmlService);
        _frameInfoService = Guard.ForNull(frameInfoService);
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
            _oldList = _xmlService.Read(xmlFileName) ?? new DrawingList();

            CompareLastWriteDateTimes();

            ReadDataFromOpenedAndModifiedDocuments();

            _xmlService.Write(_oldList, Path.Combine(_folder, "Drawinglist_copy.xml"));
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void CompareLastWriteDateTimes()
    {
        var dwgFiles = Directory.GetFiles(_folder, SearchPatternConstants.Drawings);

        foreach (var dwgFile in dwgFiles)
        {
            var frames = _oldList.Frames.Where(e => e.Filename == dwgFile);
            var files = _oldList.Files.Where(e => e.Filename == dwgFile);

            if (dwgFile == _fileName && _justSaved)
            {
                _filesToRead.Add((dwgFile, true));
                continue;
            }

            if (_documents.ContainsKey(dwgFile))
            {
                if (frames.Any())
                { _newList.Frames.AddRange(frames); }
                else if (files.Any())
                { _newList.Files.Add(files.First()); }
                _filesToRead.Add((dwgFile, false));
                continue;
            }

            if (frames.Any())
            {
                if (HasFileDateChanged(frames, File.GetLastWriteTimeUtc(dwgFile)))
                {
                    _filesToRead.Add((dwgFile, true));
                }
                else
                {
                    _newList.Frames.AddRange(frames);
                    UpdatedFrameListData.AddRange(frames);
                }
                continue;
            }

            if (files.Any())
            {
                if (HasFileDateChanged(files, File.GetLastWriteTimeUtc(dwgFile)))
                {
                    _filesToRead.Add((dwgFile, true));
                }
                else
                {
                    _newList.Files.AddRange(files);
                }
                continue;
            }

            _filesToRead.Add((dwgFile, true));
        }
    }

    private void ReadDataFromOpenedAndModifiedDocuments()
    {
        var i = 0;
        foreach (var fileToRead in _filesToRead)
        {
            Database? db = null;

            try
            {
                if (_documents.TryGetValue(fileToRead.File, out Document? value))
                {
                    db = value.Database;
                }
                else if (_folderHasWritePermission)
                {
                    ProgressChanged?.Invoke(this, new FrameSetProgressEventArgs(i++, _filesToRead.Count, Path.GetFileName(fileToRead.File)));
                    db = new Database(false, true);
                    db.ReadDwgFile(fileToRead.File, FileOpenMode.OpenForReadAndAllShare, true, "");
                }
                var frameIdCollections = XRecordObjectIdsHelper.Load(db, "FrameWorkIDs");

                if (db is not null && frameIdCollections.Count > 0)
                {
                    using var tr = db.TransactionManager.StartTransaction();
                    foreach (var pair in frameIdCollections)
                    {
                        var frame = new FrameData
                        {
                            Id = pair.Key,
                            Filename = Path.GetFileName(fileToRead.File),
                            FileDate = File.GetLastWriteTimeUtc(fileToRead.File),
                        };
                        AddHeaderData(tr, frame, pair.Value);

                        UpdatedFrameListData.Add(frame);
                        if (fileToRead.ForDrawingList)
                        {
                            _newList.Frames.Add(frame);
                        }
                    }
                    tr.Commit();
                }
                else if (fileToRead.ForDrawingList)
                {
                    _newList.Files.Add(new FileData
                    {
                        Filename = Path.GetFileName(fileToRead.File),
                        FileDate = File.GetLastWriteTimeUtc(fileToRead.File),
                    });
                }
            }
            catch
            {
                _newList.Files.Add(new FileData
                {
                    Filename = Path.GetFileName(fileToRead.File),
                    FileDate = File.GetLastWriteTimeUtc(fileToRead.File),
                });
            }
            finally
            {
                db?.Dispose();
            }
        }
    }

    private void AddHeaderData(Transaction transaction, FrameData frameData, ObjectIdCollection frameIds)
    {
        if (!_frameInfoService.HasValidData)
            return;

        var revisions = new List<Revision>();
        var hasFrame = false;


        foreach (ObjectId frameId in frameIds)
        {
            var blockReference = transaction.GetBlockReference(frameId);
            if (blockReference is null)
                continue;

            var family = string.Empty;
            if (frameId == frameIds[0])
            {
                var frameInfo = _frameInfoService.GetFrame(blockReference.Name.Split("$").First());
                if (frameInfo is null)
                    return;

                hasFrame = true;
                frameData.FrameSize = frameInfo.FrameSize;
                family = frameInfo.Family;
            }
            else
            {
                var headerInfo = _frameInfoService.GetHeader(blockReference.Name);
                if (headerInfo is null)
                    return;

                family = headerInfo.Family;
            }
            var attributeInfos = _frameInfoService.GetAttributes(family);
            var tagHandler = new TagsHandler();
            foreach (ObjectId attributeId in blockReference.AttributeCollection)
            {
                var attribute = transaction.GetAttributeReference(attributeId);
                if (attribute is null)
                    return;

                var attributeTag = tagHandler.GetUniqueTag(attribute.Tag);
                var attributeInfo = attributeInfos.FirstOrDefault(e => e.Name == attributeTag);
                if (attributeInfo is null)
                    continue;

                if (attributeInfo.Revision is null)
                {
                    PropertyHelper.Set(frameData, attributeInfo.Info, attribute.TextString);
                }
                else
                {
                    var revision = revisions.FirstOrDefault(e => e.Number == attributeInfo.Revision);
                    if (revision is null)
                    {
                        revision = new Revision(attributeInfo.Revision.Value);
                        revisions.Add(revision);
                    }
                    PropertyHelper.Set(revision, attributeInfo.Info, attribute.TextString);
                }
            }
        }

        AddLatestRevision(frameData, revisions);

        if (hasFrame)
        {
            frameData.Scale = FrameHelper.GetScaleFactor(transaction, frameIds[0]);
            if (!string.IsNullOrWhiteSpace(frameData.FrameSize))
            {
                frameData.Size = frameData.FrameSize;
            }
        }
    }

    private static bool HasFileDateChanged(IEnumerable<IFileData> data, DateTime lastWriteTimeUtc)
        => data.Any(e => e.FileDate != lastWriteTimeUtc);

    private static void AddLatestRevision(FrameData frameData, List<Revision> revisions)
    {
        var lastRevision = revisions.OrderByDescending(e => e.Date).FirstOrDefault();
        if (lastRevision is null)
            return;

        frameData.RevisionChar = lastRevision.Char;
        frameData.RevisionDate = lastRevision.Date;
        frameData.RevisionDescription = lastRevision.Description;
        frameData.RevisionDrawn = lastRevision.Drawn;
        frameData.RevisionCheck = lastRevision.Check;
        if (lastRevision.KopRev is null)
            return;

        frameData.RevisionChar = lastRevision.KopRev.LeftString(1);
        frameData.RevisionDrawn = lastRevision.KopRev.MidString(2).Trim(' ', '(', ')');
    }

    private class Revision(int number)
    {
        public int Number { get; } = number;
        public string Char { get; set; } = string.Empty;
        public DateOnly Date { get; set; } = DateOnly.MinValue;
        public string Description { get; set; } = string.Empty;
        public string Drawn { get; set; } = string.Empty;
        public string Check { get; set; } = string.Empty;
        public string KopRev { get; set; } = string.Empty;
    }
}
