using System.Globalization;
using System.Xml.Serialization;

namespace GadecCAD.Models;

[XmlRoot("GadecAutoCAD")]
public class DrawingList
{
    [XmlElement("Frames")] public List<FrameData> Frames { get; set; } = [];
    [XmlElement("Files")] public List<FileData> Files { get; set; } = [];
}

public class FileData : IFileData
{
    [XmlAttribute("Filename")] public string Filename { get; set; } = string.Empty;
    [XmlIgnore] public DateTime FileDate { get; set; }

    [XmlAttribute("Filedate")]
    public string FileDateString
    {
        get => FileDate.ToString("yyyy-MM-dd@HH.mm.ss");
        set => FileDate = DateTime.ParseExact(value, "yyyy-MM-dd@HH.mm.ss", CultureInfo.InvariantCulture);
    }
}

public class FrameData : FileData, IFileData
{
    [XmlAttribute("Num")] public string Num { get; set; } = string.Empty;
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
    [XmlIgnore] public DateOnly Date { get; set; }
    [XmlAttribute("Descr")] public string Descr { get; set; } = string.Empty;
    [XmlAttribute("Drawn")] public string Drawn { get; set; } = string.Empty;
    [XmlAttribute("LastRev_Char")] public string LastRev_Char { get; set; } = string.Empty;
    [XmlIgnore] public DateOnly LastRev_Date { get; set; }
    [XmlAttribute("LastRev_Descr")] public string LastRev_Descr { get; set; } = string.Empty;
    [XmlAttribute("LastRev_Drawn")] public string LastRev_Drawn { get; set; } = string.Empty;
    [XmlAttribute("LastRev_Check")] public string LastRev_Check { get; set; } = string.Empty;
    [XmlAttribute("Check")] public string Check { get; set; } = string.Empty;
    [XmlAttribute("Design")] public string Design { get; set; } = string.Empty;

    [XmlAttribute("Date")]
    public string DateString
    {
        get => Date.ToString("dd-MM-yyyy");
        set => Date = DateOnly.ParseExact(DateString, "dd-MM-yyyy", CultureInfo.InvariantCulture);
    }

    [XmlAttribute("LastRev_Date")]
    public string LastRev_DateString
    {
        get => LastRev_Date.ToString("dd-MM-yyyy");
        set => LastRev_Date = DateOnly.ParseExact(DateString, "dd-MM-yyyy", CultureInfo.InvariantCulture);
    }
}
