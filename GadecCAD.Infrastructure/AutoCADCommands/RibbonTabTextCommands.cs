using Autodesk.AutoCAD.Runtime;
using GadecCAD.Infrastructure.AutoCADHelpers;

namespace GadecCAD.Infrastructure.AutoCADCommands;
public static class RibbonTabTextCommands
{
    [CommandMethod("HelloGadec")]
    public static void CommandHelloGadec()
    {
        try
        {
            DocumentsHelper.ActiveEditor.WriteMessage("Hello Gadec");
        }
        catch (System.Exception ex)
        {
            Console.WriteLine(ex.Message); // TODO: Error dialog
        }
    }
}