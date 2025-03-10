using GadecCAD.Core.Models;

namespace GadecCAD.Application.Interfaces;
public interface IDrawingDataService
{
    IEnumerable<string> GetOpenDocumentNames();
    List<IDrawingData> GetDrawingData(string dwgName);
}