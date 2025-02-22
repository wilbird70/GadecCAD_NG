
namespace GadecCAD.Models;

public interface IFileData
{
    DateTime FileDate { get; set; }
    string FileDateString { get; set; }
    string Filename { get; set; }
}