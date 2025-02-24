using System.Xml.Serialization;

namespace GadecCAD.Application.Models;
[XmlRoot("FrameInfo")]
public class FrameInfo
{
    [XmlElement("FrameFilter")] public List<FrameFilter> FrameFilters { get; set; } = [];
    [XmlElement("HeaderFilter")] public List<HeaderFilter> HeaderFilters { get; set; } = [];
    [XmlElement("StampFilter")] public List<StampFilter> StampFilters { get; set; } = [];

    [XmlElement("Frame")] public List<Frame> Frames { get; set; } = [];
    [XmlElement("Header")] public List<Header> Headers { get; set; } = [];
    [XmlElement("Stamp")] public List<Stamp> Stamps { get; set; } = [];

    [XmlElement("Attribute")] public List<Attribute> Attributes { get; set; } = [];
}

public class FrameFilter
{
    [XmlAttribute("Name")] public string Name { get; set; } = string.Empty;
}

public class HeaderFilter
{
    [XmlAttribute("Name")] public string Name { get; set; } = string.Empty;
}

public class StampFilter
{
    [XmlAttribute("Name")] public string Name { get; set; } = string.Empty;
}

public class Frame
{
    [XmlAttribute("Name")] public string Name { get; set; } = string.Empty;
    [XmlAttribute("Family")] public string Family { get; set; } = string.Empty;
    [XmlAttribute("FrameSize")] public string FrameSize { get; set; } = string.Empty;
}

public class Header
{
    [XmlAttribute("Name")] public string Name { get; set; } = string.Empty;
    [XmlAttribute("Family")] public string Family { get; set; } = string.Empty;
}

public class Stamp
{
    [XmlAttribute("Name")] public string Name { get; set; } = string.Empty;
    [XmlIgnore] public bool Current { get; set; }

    [XmlAttribute("Current")]
    public string CurrentString
    {
        get => Current ? "Yes" : "No";
        set => Current = CurrentString == "Yes";
    }
}

public class Attribute
{
    [XmlAttribute("Name")] public string Name { get; set; } = string.Empty;
    [XmlAttribute("Family")] public string Family { get; set; } = string.Empty;
    [XmlAttribute("Info")] public string Info { get; set; } = string.Empty;
    [XmlIgnore] public int? Revision { get; set; }

    [XmlAttribute("Revision")]
    public string RevisionString
    {
        get => Revision.ToString() ?? string.Empty;
        set
        {
            if (int.TryParse(RevisionString, out int result))
            {
                Revision = result;
            }
        }
    }
}
