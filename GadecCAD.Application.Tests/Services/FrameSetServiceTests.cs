using GadecCAD.Application.Interfaces;
using GadecCAD.Application.Services;
using GadecCAD.Core.Models;
using GadecCAD.Data.Services;
using NSubstitute;
using NUnit.Framework;

namespace GadecCAD.Application.Tests.Services;
internal class FrameSetServiceTests
{
    private IDrawingDataService _drawingDataService = default!;
    private IFileSystemService _fileSystemService = default!;

    [Test]
    public void Test_UpdateDrawingList_returns_a_valid_list()
    {
        _drawingDataService = Substitute.For<IDrawingDataService>();
        _fileSystemService = Substitute.For<IFileSystemService>();

        var frameData1 = new FrameData { FileName = "Filename1", FileDateString = "2025-03-08@19.42.02", ClientRow1 = "Client A", DescriptionRow1 = "Description X", DateString = "29/7/2024", RevisionDateString = "11-02-2025" };
        var frameData2 = new FrameData { FileName = "Filename1", FileDateString = "2025-03-08@19.42.02", ClientRow1 = "Client A", DescriptionRow1 = "Description Y", DateString = "11-10-2024", RevisionDateString = "11-02-2025" };
        var fileData1 = new FileData { FileName = "Filename2", FileDateString = "2025-03-08@19.42.02" };

        _drawingDataService.GetOpenDocumentNames().Returns(["Filename1"]);
        _drawingDataService.GetDrawingData("Filename1").Returns([frameData1, frameData2]);
        _drawingDataService.GetDrawingData("Filename2").Returns([fileData1]);
        _fileSystemService.GetDrawingFiles(Arg.Any<string>()).Returns(["Filename1", "Filename2"]);
        _fileSystemService.GetLastWriteTimeUtc(Arg.Any<string>()).Returns(new DateTime(2025, 3, 8, 19, 42, 02));

        var result = WhenWeHandle_UpdateDrawingList("Filename2", true);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.Frames, Has.Count.EqualTo(2));
            Assert.That(result.Files, Has.Count.EqualTo(1));
        });
    }

    private FrameSetService GivenService()
    {
        return new FrameSetService(new XmlService<DrawingList>(), _drawingDataService, _fileSystemService);
    }

    private DrawingList? WhenWeHandle_UpdateDrawingList(string dwgFileName, bool isSaved = false)
    {
        var service = GivenService();
        return service.UpdateDrawingList(dwgFileName, isSaved);
    }
}
