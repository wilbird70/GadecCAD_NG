using Autodesk.AutoCAD.DatabaseServices;
using GadecCAD.Application.Extensions;

namespace GadecCAD.Application.Helpers;
public static class FrameHelper
{
    public static string GetScaleFactor(Transaction transaction, ObjectId frameId)
    {
        var frame = transaction.GetBlockReference(frameId);
        if (frame is null)
            return "1:unknown";

        var blockTableRecord = transaction.GetBlockTableRecord(frame.OwnerId);
        if (blockTableRecord is BlockTableRecord btr
            && transaction.GetLayout(btr.LayoutId) is Layout layout
            && layout.LayoutName != "Model"
            && layout.GetViewports() is ObjectIdCollection viewportIds
            && viewportIds.Count > 1)
        {
            var result = 1.0;
            for (var i = 1; i < viewportIds.Count; i++)
            {
                var viewport = transaction.GetViewport(viewportIds[i]);
                var viewportScale = 1 / viewport?.CustomScale;
                if (viewportScale is >= 1 or <= 1000 && result < viewportScale)
                {
                    result = viewportScale.Value;
                }
            }
            return $"1:{result:#.#}";
        }
        else
        {
            return frame.ScaleFactors.X >= 1 ? $"1:{frame.ScaleFactors.X:#.#}" : $"{1 / frame.ScaleFactors.X:#.#}:1";
        }
    }
}
