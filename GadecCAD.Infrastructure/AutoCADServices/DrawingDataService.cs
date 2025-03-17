using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Gadec.Common.Extensions;
using Gadec.Common.Handlers;
using Gadec.Common.Helpers;
using GadecCAD.Application.Interfaces;
using GadecCAD.Core;
using GadecCAD.Core.Models;
using GadecCAD.Data.Services;
using GadecCAD.Infrastructure.AutoCADExtensions;
using GadecCAD.Infrastructure.AutoCADHelpers;
using AutoCAD = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace GadecCAD.Infrastructure.AutoCADServices;
public class DrawingDataService : IDrawingDataService
{
    private readonly FrameInfoService _frameInfoService;
    private readonly IEnumerable<Document> _documents = [];

    public DrawingDataService(FrameInfoService frameInfoService)
    {
        _frameInfoService = Guard.ForNull(frameInfoService);
        _documents = AutoCAD.DocumentManager.Documents();
    }

    public IEnumerable<string> GetOpenDocumentNames() => _documents.Select(x => x.Name);

    public List<IDrawingData> GetDrawingData(string dwgFile, DateTime dwgDate)
    {
        var database = _documents.FirstOrDefault(e => e.Name == dwgFile)?.Database;
        var fileName = Path.GetFileName(dwgFile);
        List<IDrawingData> result = [];

        try
        {
            if (database is null)
            {
                database = new Database(false, true);
                database.ReadDwgFile(dwgFile, FileOpenMode.OpenForReadAndAllShare, true, "");
            }

            var frameIdCollections = XRecordObjectIdsHelper.Load(database, "FrameWorkIDs");

            if (database is not null && frameIdCollections.Count > 0)
            {
                using var tr = database.TransactionManager.StartTransaction();
                foreach (var pair in frameIdCollections)
                {
                    var frame = new FrameData
                    {
                        Id = pair.Key,
                        FileName = fileName,
                        FileDate = dwgDate,
                    };
                    AddHeaderData(tr, frame, pair.Value);
                    result.Add(frame);
                }
                tr.Commit();
            }
        }
        finally
        {
            database?.Dispose();
        }

        if (result.Count == 0)
        {
            result.Add(new FileData
            {
                FileName = fileName,
                FileDate = dwgDate,
            });
        }
        return result;
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
                    continue;

                hasFrame = true;
                frameData.FrameSize = frameInfo.FrameSize;
                family = frameInfo.Family;
            }
            else
            {
                var headerInfo = _frameInfoService.GetHeader(blockReference.Name);
                if (headerInfo is null)
                    continue;

                family = headerInfo.Family;
            }
            var attributeInfos = _frameInfoService.GetAttributes(family);
            var tagHandler = new TagsHandler();
            foreach (ObjectId attributeId in blockReference.AttributeCollection)
            {
                var attribute = transaction.GetAttributeReference(attributeId);
                if (attribute is null)
                    continue;

                var attributeTag = tagHandler.GetUniqueTag(attribute.Tag);
                var attributeInfo = attributeInfos.FirstOrDefault(e => e.Name == attributeTag);
                if (attributeInfo is null)
                    continue;

                if (attributeInfo.Revision == 0)
                {
                    PropertyHelper.Set(frameData, attributeInfo.Info, attribute.TextString);
                }
                else
                {
                    var revision = revisions.FirstOrDefault(e => e.Number == attributeInfo.Revision);
                    if (revision is null)
                    {
                        revision = new Revision(attributeInfo.Revision);
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

    private static void AddLatestRevision(FrameData frameData, List<Revision> revisions)
    {
        var lastRevision = revisions.OrderByDescending(e => e.Date).FirstOrDefault();
        if (lastRevision is null || lastRevision.Date == DateOnly.MinValue)
            return;

        frameData.RevisionChar = lastRevision.Char;
        frameData.RevisionDate = lastRevision.Date;
        frameData.RevisionDescription = lastRevision.Description;
        frameData.RevisionDrawn = lastRevision.Drawn;
        frameData.RevisionCheck = lastRevision.Check;
        if (string.IsNullOrWhiteSpace(lastRevision.KopRev))
            return;

        frameData.RevisionChar = lastRevision.KopRev.LeftString(1);
        frameData.RevisionDrawn = lastRevision.KopRev.MidString(2).Trim(' ', '(', ')');
    }

    private class Revision(int number)
    {
        public int Number { get; } = number;
        public string? Char { get; set; }
        public DateOnly? Date { get; set; }
        public string? Description { get; set; }
        public string? Drawn { get; set; }
        public string? Check { get; set; }
        public string? KopRev { get; set; }
        public string? DateString { get => null; set => Date = DateOnlyHelper.FromString(value); }
    }
}
