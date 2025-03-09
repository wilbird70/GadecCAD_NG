using GadecCAD.Application.Interfaces;
using GadecCAD.Core.Models;
using GadecCAD.Data.Services;

namespace GadecCAD.Application.Tests.Mocks;
internal class DrawingDataServiceMock : IDrawingDataService
{
    public DrawingDataServiceMock(FrameInfoService frameInfoService)
    {
        //_frameInfoService = Guard.ForNull(frameInfoService);
        //_documents = AutoCAD.DocumentManager.Documents();
    }
    public List<string> OpenDocuments => [];
    public List<IDrawingData> GetDrawingData(string dwgName) => [];
}
