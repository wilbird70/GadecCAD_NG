using GadecCAD.Application.Interfaces;
using GadecCAD.Infrastructure.AutoCADHelpers;

namespace GadecCAD.Infrastructure.AutoCADServices;
public class EditorService : IEditorService
{
    public void WriteMessage(string message)
    {
        DocumentsHelper.ActiveEditor.WriteMessage(message);
    }
}
