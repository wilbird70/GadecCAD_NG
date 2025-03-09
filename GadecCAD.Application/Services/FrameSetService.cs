using Gadec.Common.Helpers;
using GadecCAD.Application.EventArguments;
using GadecCAD.Application.Interfaces;
using GadecCAD.Core;
using GadecCAD.Core.Models;
using GadecCAD.Data.Services;
using Drawing = (string FileName, bool ClosedOrSaved);

namespace GadecCAD.Application.Services;
public class FrameSetService
{
    public List<FrameData> UpdatedFrameList { get; } = [];
    public event EventHandler<ProgressChangedEventArgs>? ProgressChanged;

    private readonly XmlService<DrawingList> _xmlService;
    private readonly IDrawingDataService _frameDataService;
    private readonly IFileService _fileService;

    private string _currentFolder = string.Empty;
    private string _dwgFileName = string.Empty;
    private bool _isSaved;
    private List<string> _documents = [];

    private readonly DrawingList _drawingList = new();

    public FrameSetService(XmlService<DrawingList> xmlService, IDrawingDataService frameDataService, IFileService fileService)
    {
        _xmlService = Guard.ForNull(xmlService);
        _frameDataService = Guard.ForNull(frameDataService);
        _fileService = Guard.ForNull(fileService);
    }

    public DrawingList? UpdateDrawingList(string dwgFileName, bool isSaved = false)
    {
        _dwgFileName = dwgFileName;
        _currentFolder = Path.GetDirectoryName(dwgFileName) ?? throw new ArgumentException("Not able to parse path", nameof(dwgFileName));
        _isSaved = isSaved;
        _documents = _frameDataService.OpenDocuments;

        try
        {
            var xmlFileName = Path.Combine(_currentFolder, "Drawings.xml");

            var currentDrawingList = _xmlService.Read(xmlFileName) ?? new DrawingList();
            var dwgFilesToRead = CompareLastWriteDateTimes(currentDrawingList);
            ReadDataFromDocuments(dwgFilesToRead);

            _drawingList.Frames = [.. _drawingList.Frames.OrderBy(e => e.FileName).ThenBy(e => e.Drawing).ThenBy(e => e.Sheet)];
            _drawingList.Files = [.. _drawingList.Files.OrderBy(e => e.FileName)];

            return _drawingList;
        }
        catch
        {
            return null;
        }
    }

    public void SaveDrawingList()
    {
        if (FileSystemHelper.FolderHasWritePermission(_currentFolder))
        {
            _xmlService.Write(_drawingList, Path.Combine(_currentFolder, "Drawings.xml"));
        }
    }

    private List<Drawing> CompareLastWriteDateTimes(DrawingList currentDrawingList)
    {
        List<Drawing> result = [];
        foreach (var dwgFile in _fileService.GetDrawingFiles(_currentFolder))
        {
            var fileName = Path.GetFileName(dwgFile);
            var frames = currentDrawingList.Frames.Where(e => e.FileName == fileName).ToList();
            var files = currentDrawingList.Files.Where(e => e.FileName == fileName).ToList();

            if (dwgFile == _dwgFileName && _isSaved)
            {
                result.Add((dwgFile, true));
                continue;
            }

            if (_documents.Contains(dwgFile))
            {
                if (frames.Count != 0)
                {
                    _drawingList.Frames.AddRange(frames);
                }
                else if (files.Count != 0)
                {
                    _drawingList.Files.Add(files[0]);
                }
                result.Add((dwgFile, false));
                continue;
            }

            if (frames.Count != 0)
            {
                if (HasFileDateChanged(frames, File.GetLastWriteTimeUtc(dwgFile)))
                {
                    result.Add((dwgFile, true));
                }
                else
                {
                    _drawingList.Frames.AddRange(frames);
                    UpdatedFrameList.AddRange(frames);
                }
                continue;
            }

            if (files.Count != 0)
            {
                if (HasFileDateChanged(files, File.GetLastWriteTimeUtc(dwgFile)))
                {
                    result.Add((dwgFile, true));
                }
                else
                {
                    _drawingList.Files.AddRange(files);
                }
                continue;
            }

            result.Add((dwgFile, true));
        }
        return result;
    }

    private void ReadDataFromDocuments(List<Drawing> drawings)
    {
        var i = 0;
        foreach (var (fileName, ClosedOrSaved) in drawings)
        {
            ProgressChanged?.Invoke(this, new(i++, drawings.Count, Path.GetFileName(fileName)));
            foreach (var drawingData in _frameDataService.GetDrawingData(fileName))
            {
                if (drawingData is FrameData frameData)
                {
                    UpdatedFrameList.Add(frameData);
                    if (ClosedOrSaved)
                    {
                        _drawingList.Frames.Add(frameData);
                    }
                }
                else if (drawingData is FileData fileData && ClosedOrSaved)
                {
                    _drawingList.Files.Add(fileData);
                }
            }
        }
    }

    private static bool HasFileDateChanged(IEnumerable<IDrawingData> data, DateTime lastWriteTimeUtc) => data.Any(e => e.FileDate != lastWriteTimeUtc);
}
