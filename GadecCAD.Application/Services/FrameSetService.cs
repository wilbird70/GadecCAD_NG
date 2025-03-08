using Autodesk.AutoCAD.ApplicationServices;
using Gadec.Common.Helpers;
using GadecCAD.Application.Constants;
using GadecCAD.Application.EventArguments;
using GadecCAD.Application.Extensions;
using GadecCAD.Application.Models;
using AutoCAD = Autodesk.AutoCAD.ApplicationServices.Application;
using Drawing = (string FileName, bool Open);

namespace GadecCAD.Application.Services;
public class FrameSetService
{
    public List<FrameData> UpdatedFrameList { get; } = [];
    public event EventHandler<ProgressChangedEventArgs>? ProgressChanged;

    private readonly XmlService<DrawingList> _xmlService;
    private readonly DrawingDataService _frameDataService;

    private string _currentFolder = string.Empty;
    private string _dwgFileName = string.Empty;
    private bool _isSaved;
    private List<Document> _documents = [];

    private readonly DrawingList _drawingList = new();

    public FrameSetService(XmlService<DrawingList> xmlService, DrawingDataService frameDataService)
    {
        _xmlService = Guard.ForNull(xmlService);
        _frameDataService = Guard.ForNull(frameDataService);
    }

    public DrawingList? UpdateDrawingList(string dwgFileName, bool isSaved = false)
    {
        _dwgFileName = dwgFileName;
        _currentFolder = Path.GetDirectoryName(dwgFileName) ?? throw new ArgumentException("Not able to parse path", nameof(dwgFileName));
        _isSaved = isSaved;
        _documents = AutoCAD.DocumentManager.Documents();

        try
        {
            var xmlFileName = Path.Combine(_currentFolder, "Drawings.xml");

            var currentDrawingList = _xmlService.Read(xmlFileName) ?? new DrawingList();
            var dwgFilesToRead = CompareLastWriteDateTimes(currentDrawingList);
            ReadDataFromDocuments(dwgFilesToRead);

            var sorted = new DrawingList
            {
                Frames = _drawingList.Frames.OrderBy(e => e.Filename).ThenBy(e => e.Drawing).ThenBy(e => e.Sheet).ToList(),
                Files = _drawingList.Files.OrderBy(e => e.Filename).ToList()
            };

            if (FileSystemHelper.FolderHasWritePermission(_currentFolder))
            {
                _xmlService.Write(sorted, Path.Combine(_currentFolder, "Drawings.xml"));
            }

            return sorted;
        }
        catch
        {
            return null;
        }
    }

    private List<Drawing> CompareLastWriteDateTimes(DrawingList currentDrawingList)
    {
        List<Drawing> result = [];
        foreach (var dwgFile in Directory.GetFiles(_currentFolder, SearchPatternConstants.Drawings))
        {
            var fileName = Path.GetFileName(dwgFile);
            var frames = currentDrawingList.Frames.Where(e => e.Filename == fileName).ToList();
            var files = currentDrawingList.Files.Where(e => e.Filename == fileName).ToList();

            if (dwgFile == _dwgFileName && _isSaved)
            {
                result.Add((dwgFile, true));
                continue;
            }

            if (_documents.Any(e => e.Name == dwgFile))
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
        foreach (var (fileName, open) in drawings)
        {
            ProgressChanged?.Invoke(this, new(i++, drawings.Count, Path.GetFileName(fileName)));
            using var database = _documents.FirstOrDefault(e => e.Name == fileName)?.Database;
            foreach (var drawingData in _frameDataService.GetDrawingData(fileName, database))
            {
                if (drawingData is FrameData frameData)
                {
                    UpdatedFrameList.Add(frameData);
                    if (!open)
                    {
                        _drawingList.Frames.Add(frameData);
                    }
                }
                else if (drawingData is FileData fileData && !open)
                {
                    _drawingList.Files.Add(fileData);
                }
            }
        }
    }

    private static bool HasFileDateChanged(IEnumerable<IDrawingData> data, DateTime lastWriteTimeUtc) => data.Any(e => e.FileDate != lastWriteTimeUtc);
}
