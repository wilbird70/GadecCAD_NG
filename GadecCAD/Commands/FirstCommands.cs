using Autodesk.AutoCAD.Runtime;
using GadecCAD.Application.Handlers;
using GadecCAD.Core.Models;
using MediatR;

namespace GadecCAD.Commands;
public static class FirstCommands
{
    private static ISender? _mediator;
    private static ISender Mediator => _mediator ??= ApplicationServices.GetRequiredService<ISender>();

    public static List<FrameData>? UpdateDrawingList(string dwgFileName, bool isSaved = false)
    {
        var task = Mediator.Send(new UpdateDrawingList(dwgFileName, isSaved));
        return task.Result;
    }

    [CommandMethod("HelloGadec")]
    public static void CommandHelloGadec()
    {
        Mediator.Send(new HelloGadec());
    }
}
