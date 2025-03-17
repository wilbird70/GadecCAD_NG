namespace GadecCAD.Data.Services;

public interface IXmlService<T>
{
    T? Read(string filePath);
    void Write(T data, string filePath);
}