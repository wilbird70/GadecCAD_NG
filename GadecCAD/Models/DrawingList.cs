using System.Xml.Serialization;

namespace GadecCAD.Models;

[XmlRoot("GadecAutoCAD")]
public class DrawingList
{
    [XmlElement("Frames")] public List<FrameData> Frames { get; set; } = [];
    [XmlElement("Files")] public List<FileData> Files { get; set; } = [];
}

public class FrameData
{
    [XmlAttribute("Filename")] public string Filename { get; set; } = string.Empty;
    [XmlAttribute("Num")] public string Num { get; set; } = string.Empty;
    [XmlAttribute("Filedate")] public string Filedate { get; set; } = string.Empty;
    [XmlAttribute("Dossier")] public string Dossier { get; set; } = string.Empty;
    [XmlAttribute("Drawing")] public string Drawing { get; set; } = string.Empty;
    [XmlAttribute("Sheet")] public string Sheet { get; set; } = string.Empty;
    [XmlAttribute("Descr1")] public string Descr1 { get; set; } = string.Empty;
    [XmlAttribute("Descr2")] public string Descr2 { get; set; } = string.Empty;

    [XmlAttribute("Descr3")] public string Descr3 { get; set; } = string.Empty;
    [XmlAttribute("Client1")] public string Client1 { get; set; } = string.Empty;
    [XmlAttribute("Client2")] public string Client2 { get; set; } = string.Empty;
    [XmlAttribute("Client3")] public string Client3 { get; set; } = string.Empty;
    [XmlAttribute("Client4")] public string Client4 { get; set; } = string.Empty;
    [XmlAttribute("Project")] public string Project { get; set; } = string.Empty;
    [XmlAttribute("Rev")] public string Rev { get; set; } = string.Empty;
    [XmlAttribute("FrameSize")] public string FrameSize { get; set; } = string.Empty;
    [XmlAttribute("Size")] public string Size { get; set; } = string.Empty;
    [XmlAttribute("Scale")] public string Scale { get; set; } = string.Empty;
    [XmlAttribute("Char")] public string Char { get; set; } = string.Empty;
    [XmlAttribute("Date")] public string Date { get; set; } = string.Empty;
    [XmlAttribute("Descr")] public string Descr { get; set; } = string.Empty;
    [XmlAttribute("Drawn")] public string Drawn { get; set; } = string.Empty;
    [XmlAttribute("LastRev_Char")] public string LastRev_Char { get; set; } = string.Empty;
    [XmlAttribute("LastRev_Date")] public string LastRev_Date { get; set; } = string.Empty;
    [XmlAttribute("LastRev_Descr")] public string LastRev_Descr { get; set; } = string.Empty;
    [XmlAttribute("LastRev_Drawn")] public string LastRev_Drawn { get; set; } = string.Empty;
    [XmlAttribute("LastRev_Check")] public string LastRev_Check { get; set; } = string.Empty;
    [XmlAttribute("Check")] public string Check { get; set; } = string.Empty;
    [XmlAttribute("Design")] public string Design { get; set; } = string.Empty;
}

public class FileData
{
    [XmlAttribute("Filename")] public string Filename { get; set; } = string.Empty;
    [XmlAttribute("Filedate")] public string Filedate { get; set; } = string.Empty;
}
