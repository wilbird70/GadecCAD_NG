using GadecCAD.Application.Models;
using GadecCAD.Application.Services;
using GadecCAD.Application.Tests.Mocks;
using NSubstitute;
using NUnit.Framework;

namespace GadecCAD.Application.Tests.Services;
internal class FrameSetServiceTests
{
    [Test]
    public void Test_UpdateDrawingList_returns_a_valid_list()
    {
        var drawingDataService = Substitute.For<IDrawingDataService>();
        drawingDataService.OpenDocuments.Returns([]);

        var frameData1 = new FrameData { FileName = "Filename", FileDateString = "2025-03-08@19.42.02", ClientRow1 = "Client A", DescriptionRow1 = "Description B", DateString = "29/7/2024", RevisionDateString = "11-02-2025" };
        var frameData2 = new FrameData { FileName = "Filename", FileDateString = "2025-03-08@19.42.02", ClientRow1 = "Client A", DescriptionRow1 = "Description B", DateString = "11-10-2024", RevisionDateString = "11-02-2025" };
        var fileData1 = new FileData { FileName = "Filename", FileDateString = "2025-03-08@19.42.02" };

        drawingDataService.GetDrawingData(Arg.Any<string>()).Returns([]);

        var result = WhenWeHandle_UpdateDrawingList("any", false);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Frames, Has.Count.EqualTo(2));
        Assert.That(result.Frames, Has.Count.EqualTo(1));
    }

    private FrameSetService GivenService()
    {
        var frameInfoService = new FrameInfoService(new XmlService<FrameInfo>());
        var drawingDataService = new DrawingDataServiceMock(frameInfoService);
        return new FrameSetService(new XmlService<DrawingList>(), drawingDataService);
    }

    private DrawingList? WhenWeHandle_UpdateDrawingList(string dwgFileName, bool isSaved = false)
    {
        var service = GivenService();
        return service.UpdateDrawingList(dwgFileName, isSaved);
    }
}
