namespace GadecCAD.Application.Models;
public interface IDrawingData
{
    DateTime FileDate { get; set; }
    string FileDateString { get; set; }
    string Filename { get; set; }
}