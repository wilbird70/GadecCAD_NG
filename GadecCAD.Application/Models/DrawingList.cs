using GadecCAD.Application.Helpers;
using System.Globalization;
using System.Xml.Serialization;

namespace GadecCAD.Application.Models;
[XmlRoot("GadecAutoCAD")]
public class DrawingList
{
    [XmlElement("Frames")] public List<FrameData> Frames { get; set; } = [];
    [XmlElement("Files")] public List<FileData> Files { get; set; } = [];
}

public class FileData : IDrawingData
{
    [XmlAttribute("Filename")] public string FileName { get; set; } = string.Empty;
    [XmlIgnore] public DateTime FileDate { get; set; }

    [XmlAttribute("Filedate")]
    public string FileDateString
    {
        get => FileDate.ToString("yyyy-MM-dd@HH.mm.ss");
        set => FileDate = DateTime.ParseExact(value, "yyyy-MM-dd@HH.mm.ss", CultureInfo.InvariantCulture);
    }
}

public class FrameData : FileData, IDrawingData
{
    [XmlAttribute("Num")] public string Id { get; set; } = string.Empty;
    [XmlAttribute("Dossier")] public string Dossier { get; set; } = string.Empty;
    [XmlAttribute("Drawing")] public string Drawing { get; set; } = string.Empty;
    [XmlAttribute("Sheet")] public string Sheet { get; set; } = string.Empty;

    [XmlAttribute("Descr1")] public string DescriptionRow1 { get; set; } = string.Empty;
    [XmlAttribute("Descr2")] public string DescriptionRow2 { get; set; } = string.Empty;
    [XmlAttribute("Descr3")] public string DescriptionRow3 { get; set; } = string.Empty;
    [XmlAttribute("Descr4")] public string DescriptionRow4 { get; set; } = string.Empty;

    [XmlAttribute("Client1")] public string ClientRow1 { get; set; } = string.Empty;
    [XmlAttribute("Client2")] public string ClientRow2 { get; set; } = string.Empty;
    [XmlAttribute("Client3")] public string ClientRow3 { get; set; } = string.Empty;
    [XmlAttribute("Client4")] public string ClientRow4 { get; set; } = string.Empty;

    [XmlAttribute("Project")] public string Project { get; set; } = string.Empty;
    [XmlAttribute("Design")] public string Design { get; set; } = string.Empty;
    [XmlAttribute("Rev")] public string Rev { get; set; } = string.Empty;
    [XmlAttribute("Scale")] public string Scale { get; set; } = string.Empty;
    [XmlAttribute("FrameSize")] public string FrameSize { get; set; } = string.Empty;
    [XmlAttribute("Size")] public string Size { get; set; } = string.Empty;

    [XmlAttribute("Char")] public string Char { get; set; } = string.Empty;
    [XmlAttribute("Date")] public string DateString { get => DateOnlyHelper.ToString(Date); set => Date = DateOnlyHelper.FromString(value); }
    [XmlAttribute("Descr")] public string Description { get; set; } = string.Empty;
    [XmlAttribute("Drawn")] public string Drawn { get; set; } = string.Empty;
    [XmlAttribute("Check")] public string Check { get; set; } = string.Empty;

    [XmlAttribute("LastRev_Char")] public string RevisionChar { get; set; } = "0";
    [XmlAttribute("LastRev_Date")] public string RevisionDateString { get => DateOnlyHelper.ToString(RevisionDate); set => RevisionDate = DateOnlyHelper.FromString(value); }
    [XmlAttribute("LastRev_Descr")] public string RevisionDescription { get; set; } = string.Empty;
    [XmlAttribute("LastRev_Drawn")] public string RevisionDrawn { get; set; } = string.Empty;
    [XmlAttribute("LastRev_Check")] public string RevisionCheck { get; set; } = string.Empty;

    [XmlIgnore] public DateOnly Date { get; set; }
    [XmlIgnore] public DateOnly RevisionDate { get; set; }
}
