using GadecCAD.Application.EventArguments;
using GadecCAD.Application.Interfaces;
using GadecCAD.Core;
using GadecCAD.Core.Models;
using GadecCAD.Data.Services;
using MediatR;
using Drawing = (string FileName, System.DateTime FileDate, bool ClosedOrSaved);

namespace GadecCAD.Application.Handlers;
public record UpdateDrawingList(string DwgFileName, bool IsSaved) : IRequest<List<FrameData>?>;

public class UpdateDrawingListHandler : IRequestHandler<UpdateDrawingList, List<FrameData>?>
{
    public event EventHandler<ProgressChangedEventArgs>? ProgressChanged;

    private readonly IXmlService<DrawingList> _xmlService;
    private readonly IDrawingDataService _drawingDataService;
    private readonly IFileSystemService _fileSystemService;

    private string _dwgFileName = string.Empty;
    private bool _isSaved;

    private readonly DrawingList _drawingList = new();
    private readonly List<FrameData> _updatedFrameList = [];

    public UpdateDrawingListHandler(IXmlService<DrawingList> xmlService, IDrawingDataService drawingDataService, IFileSystemService fileSystemService)
    {
        _xmlService = Guard.ForNull(xmlService);
        _drawingDataService = Guard.ForNull(drawingDataService);
        _fileSystemService = Guard.ForNull(fileSystemService);
    }

    public Task<List<FrameData>?> Handle(UpdateDrawingList request, CancellationToken cancellationToken = default)
    {
        _dwgFileName = request.DwgFileName;
        _isSaved = request.IsSaved;
        var currentFolder = Path.GetDirectoryName(request.DwgFileName)
            ?? throw new ArgumentException($"Not able to parse path from: {request.DwgFileName}");

        try
        {
            var xmlFileName = Path.Combine(currentFolder, "Test.xml");

            var currentDrawingList = _xmlService.Read(xmlFileName) ?? new();
            var dwgFilesToRead = CheckAgainstDocuments(currentFolder, currentDrawingList);
            ReadDataFromDocuments(dwgFilesToRead);

            _drawingList.Frames = [.. _drawingList.Frames.OrderBy(e => e.FileName).ThenBy(e => e.Drawing).ThenBy(e => e.Sheet)];
            _drawingList.Files = [.. _drawingList.Files.OrderBy(e => e.FileName)];

            if (_fileSystemService.FolderHasWritePermission(currentFolder))
            {
                _xmlService.Write(_drawingList, Path.Combine(currentFolder, "Test.xml"));
            }

            return Task.FromResult<List<FrameData>?>(_updatedFrameList);
        }
        catch
        {
            return Task.FromResult<List<FrameData>?>(null);
        }
    }

    private List<Drawing> CheckAgainstDocuments(string currentFolder, DrawingList currentDrawingList)
    {
        var documents = _drawingDataService.GetOpenDocumentNames();

        List<Drawing> result = [];
        foreach (var (dwgFile, dwgDate) in _fileSystemService.GetDrawingFiles(currentFolder))
        {
            var fileName = Path.GetFileName(dwgFile);
            var frames = currentDrawingList.Frames.Where(e => e.FileName == fileName).ToList();
            var files = currentDrawingList.Files.Where(e => e.FileName == fileName).ToList();

            if (dwgFile == _dwgFileName && _isSaved)
            {
                result.Add((dwgFile, dwgDate, true));
                continue;
            }

            if (documents.Contains(dwgFile))
            {
                if (frames.Count != 0)
                {
                    _drawingList.Frames.AddRange(frames);
                }
                else if (files.Count != 0)
                {
                    _drawingList.Files.Add(files[0]);
                }
                result.Add((dwgFile, dwgDate, false));
                continue;
            }

            if (frames.Count != 0)
            {
                if (HasFileDateChanged(frames, dwgDate))
                {
                    result.Add((dwgFile, dwgDate, true));
                }
                else
                {
                    _drawingList.Frames.AddRange(frames);
                    _updatedFrameList.AddRange(frames);
                }
                continue;
            }

            if (files.Count != 0)
            {
                if (HasFileDateChanged(files, dwgDate))
                {
                    result.Add((dwgFile, dwgDate, true));
                }
                else
                {
                    _drawingList.Files.Add(files[0]);
                }
                continue;
            }

            result.Add((dwgFile, dwgDate, true));
        }
        return result;
    }

    private void ReadDataFromDocuments(List<Drawing> drawings)
    {
        var i = 0;
        foreach (var (dwgFile, dwgDate, closedOrSaved) in drawings)
        {
            ProgressChanged?.Invoke(this, new(i++, drawings.Count, Path.GetFileName(dwgFile)));
            foreach (var drawingData in _drawingDataService.GetDrawingData(dwgFile, dwgDate))
            {
                if (drawingData is FrameData frameData)
                {
                    _updatedFrameList.Add(frameData);
                    if (closedOrSaved)
                    {
                        _drawingList.Frames.Add(frameData);
                    }
                }
                else if (drawingData is FileData fileData && closedOrSaved)
                {
                    _drawingList.Files.Add(fileData);
                }
            }
        }
    }

    private static bool HasFileDateChanged(IEnumerable<IDrawingData> data, DateTime lastWriteTimeUtc) => data.Any(e => e.FileDate != lastWriteTimeUtc);
}
