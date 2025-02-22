using System.Xml;
using System.Xml.Serialization;

namespace GadecCAD.Services;
public class XmlService
{
    public T Read<T>(string filePath)
    {
        var serializer = new XmlSerializer(typeof(T));
        using var reader = new StreamReader(filePath);
        return (T)serializer.Deserialize(reader)!;
    }

    public void Write<T>(T data, string filePath)
    {
        var serializer = new XmlSerializer(typeof(T));
        var settings = new XmlWriterSettings
        {
            Indent = true,
            OmitXmlDeclaration = false,
            Encoding = new System.Text.UTF8Encoding(false)
        };

        using var writer = new StreamWriter(filePath);
        using var xmlWriter = XmlWriter.Create(writer, settings);
        serializer.Serialize(xmlWriter, data);
    }
}
