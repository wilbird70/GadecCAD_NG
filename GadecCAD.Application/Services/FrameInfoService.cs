using GadecCAD.Application.Models;

namespace GadecCAD.Application.Services;
public class FrameInfoService
{
    private readonly FrameInfo? _frameInfo;

    public FrameInfoService(XmlService xmlService)
    {
        var service = Guard.ForNull(xmlService);
        _frameInfo = service.Read<FrameInfo>(Path.Combine("Resources", "FrameInfo.xml"));
    }

    public bool HasValidData => _frameInfo?.Frames is not null && _frameInfo.Headers is not null;

    public Frame GetFrame(string name) => _frameInfo?.Frames.FirstOrDefault(e => e.Name == name) ?? new();
    public Header GetHeader(string name) => _frameInfo?.Headers.FirstOrDefault(e => e.Name == name) ?? new();
    public List<Models.Attribute> GetAttributes(string family) => _frameInfo?.Attributes.Where(e => e.Family == family).ToList() ?? [];
}
