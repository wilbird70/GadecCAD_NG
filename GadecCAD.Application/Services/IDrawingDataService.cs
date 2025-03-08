using GadecCAD.Application.Models;

namespace GadecCAD.Application.Services;
public interface IDrawingDataService
{
    List<string> OpenDocuments { get; }
    List<IDrawingData> GetDrawingData(string dwgName);
}