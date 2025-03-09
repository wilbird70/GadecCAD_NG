using GadecCAD.Core.Models;

namespace GadecCAD.Application.Interfaces;
public interface IDrawingDataService
{
    List<string> OpenDocuments { get; }
    List<IDrawingData> GetDrawingData(string dwgName);
}