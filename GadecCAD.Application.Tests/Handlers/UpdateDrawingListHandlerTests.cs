using GadecCAD.Application.Handlers;
using GadecCAD.Application.Interfaces;
using GadecCAD.Core.Models;
using GadecCAD.Data.Services;
using NSubstitute;
using NUnit.Framework;

namespace GadecCAD.Application.Tests.Handlers;
internal class UpdateDrawingListHandlerTests
{
    private IDrawingDataService _drawingDataService = default!;
    private IFileSystemService _fileSystemService = default!;
    private IXmlService<DrawingList> _xmlService = default!;

    [Test]
    public void Test_UpdateDrawingListHandler_returns_a_valid_list()
    {
        _drawingDataService = Substitute.For<IDrawingDataService>();
        _fileSystemService = Substitute.For<IFileSystemService>();
        _xmlService = Substitute.For<IXmlService<DrawingList>>();

        var frameData1 = new FrameData { FileName = "Filename1", FileDateString = "2025-03-08 19:42:02", ClientRow1 = "Client A", DescriptionRow1 = "Description X", DateString = "29/7/2024", RevisionDateString = "11-02-2025" };
        var frameData2 = new FrameData { FileName = "Filename1", FileDateString = "2025-03-08 19:42:02", ClientRow1 = "Client A", DescriptionRow1 = "Description Y", DateString = "11-10-2024", RevisionDateString = "11-02-2025" };
        var fileData1 = new FileData { FileName = "Filename2", FileDateString = "2025-03-08 19:42:02" };
        var dateTime = new DateTime(2025, 3, 8, 19, 42, 02);

        _drawingDataService.GetOpenDocumentNames().Returns(["Filename1"]);
        _drawingDataService.GetDrawingData("Filename1", Arg.Any<DateTime>()).Returns([frameData1, frameData2]);
        _drawingDataService.GetDrawingData("Filename2", Arg.Any<DateTime>()).Returns([fileData1]);
        _fileSystemService.GetDrawingFiles(Arg.Any<string>()).Returns([("Filename1", dateTime), ("Filename2", dateTime)]);
        _fileSystemService.FolderHasWritePermission(Arg.Any<string>()).Returns(true);
        _xmlService.Read(Arg.Any<string>()).Returns(new DrawingList());

        var request = new UpdateDrawingList("Filename1", true);
        var result = WhenWeHandle_UpdateDrawingList(request);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result, Has.Count.EqualTo(2));
        });
    }

    private List<FrameData>? WhenWeHandle_UpdateDrawingList(UpdateDrawingList request)
    {
        var handler = new UpdateDrawingListHandler(_xmlService, _drawingDataService, _fileSystemService);
        return handler.Handle(request).Result;
    }
}
