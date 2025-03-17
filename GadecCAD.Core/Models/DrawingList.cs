using Gadec.Common.Helpers;
using System.Xml.Serialization;

namespace GadecCAD.Core.Models;
[XmlRoot("GadecAutoCAD")]
public class DrawingList
{
    [XmlElement("Frames")] public List<FrameData> Frames { get; set; } = [];
    [XmlElement("Files")] public List<FileData> Files { get; set; } = [];
}

public class FileData : IDrawingData
{
    [XmlAttribute("Filename")] public string FileName { get; set; } = string.Empty;
    [XmlAttribute("Filedate")] public string FileDateString { get => DateHelper.ToString(FileDate); set => FileDate = DateHelper.DateTimeFromString(value); }

    [XmlIgnore] public DateTime FileDate { get; set; }
}

public class FrameData : FileData, IDrawingData
{
    [XmlAttribute("Num")] public string Id { get; set; } = string.Empty;
    [XmlAttribute("Dossier")] public string? Dossier { get; set; }
    [XmlAttribute("Drawing")] public string? Drawing { get; set; }
    [XmlAttribute("Sheet")] public string? Sheet { get; set; }

    [XmlAttribute("Descr1")] public string? DescriptionRow1 { get; set; }
    [XmlAttribute("Descr2")] public string? DescriptionRow2 { get; set; }
    [XmlAttribute("Descr3")] public string? DescriptionRow3 { get; set; }
    [XmlAttribute("Descr4")] public string? DescriptionRow4 { get; set; }

    [XmlAttribute("Client1")] public string? ClientRow1 { get; set; }
    [XmlAttribute("Client2")] public string? ClientRow2 { get; set; }
    [XmlAttribute("Client3")] public string? ClientRow3 { get; set; }
    [XmlAttribute("Client4")] public string? ClientRow4 { get; set; }

    [XmlAttribute("Project")] public string? Project { get; set; }
    [XmlAttribute("Design")] public string? Design { get; set; }
    [XmlAttribute("Rev")] public string? Rev { get; set; }
    [XmlAttribute("Scale")] public string? Scale { get; set; }
    [XmlAttribute("FrameSize")] public string? FrameSize { get; set; }
    [XmlAttribute("Size")] public string? Size { get; set; }

    [XmlAttribute("Char")] public string? Char { get; set; }
    [XmlAttribute("Date")] public string? DateString { get => DateHelper.ToString(Date); set => Date = DateHelper.DateOnlyFromString(value); }
    [XmlAttribute("Descr")] public string? Description { get; set; }
    [XmlAttribute("Drawn")] public string? Drawn { get; set; }
    [XmlAttribute("Check")] public string? Check { get; set; }

    [XmlAttribute("LastRev_Char")] public string? RevisionChar { get; set; } = "0";
    [XmlAttribute("LastRev_Date")] public string? RevisionDateString { get => DateHelper.ToString(RevisionDate); set => RevisionDate = DateHelper.DateOnlyFromString(value); }
    [XmlAttribute("LastRev_Descr")] public string? RevisionDescription { get; set; }
    [XmlAttribute("LastRev_Drawn")] public string? RevisionDrawn { get; set; }
    [XmlAttribute("LastRev_Check")] public string? RevisionCheck { get; set; }

    [XmlIgnore] public DateOnly? Date { get; set; }
    [XmlIgnore] public DateOnly? RevisionDate { get; set; }
}
