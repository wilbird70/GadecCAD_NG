using GadecCAD.Application.Interfaces;
using GadecCAD.Core;
using MediatR;

namespace GadecCAD.Application.Handlers;
public record HelloGadec() : IRequest;

public class HelloGadecHandler : IRequestHandler<HelloGadec>
{
    private readonly IEditorService _editorService;

    public HelloGadecHandler(IEditorService editorService)
    {
        _editorService = Guard.ForNull(editorService);
    }

    public Task Handle(HelloGadec request, CancellationToken cancellationToken)
    {
        _editorService.WriteMessage("Hello Gadec");

        return Task.CompletedTask;
    }
}
