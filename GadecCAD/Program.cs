using GadecCAD.Models;
using GadecCAD.Services;

namespace GadecCAD;
public class Program
{
    public static void Main()
    {
        string xmlPath = "C:\\Data\\_Private\\DeserializeXmlTestApp\\Drawinglist.xml";
        var xml = new XmlConverter();

        var drawingList = XmlConverter.Read<DrawingList>(xmlPath);

        Console.WriteLine($"Aantal Frames: {drawingList.Frames.Count}");
        Console.WriteLine($"Eerste Frame Filename: {drawingList.Frames[0].Filename}");
        Console.WriteLine($"Aantal Files: {drawingList.Files.Count}");
        Console.WriteLine($"Eerste File Filename: {drawingList.Files[0].Filename}");


        var frames = drawingList.Frames;
        var files = drawingList.Files;

        var test = frames.Select(e => e.Filename).ToList();

        string xmlCopy = "C:\\Data\\_Private\\DeserializeXmlTestApp\\DrawinglistCopy.xml";
        XmlConverter.Write(drawingList, xmlCopy);
    }
}
