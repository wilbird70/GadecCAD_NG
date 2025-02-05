'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.Geometry
Imports GadecCAD_NG.Extensions

''' <summary>
''' Provides a methode for setting the view for frames.
''' </summary>
Public Class FrameViewHelper

    ''' <summary>
    ''' Sets the view around a frame in the drawing viewer.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="frameId">The objectid of the frame.</param>
    Public Shared Sub SetView(document As Document, frameId As ObjectId)
        Dim row = document.FrameData.Rows.Find(frameId)
        If IsNothing(row) Then Exit Sub

        Dim extents = row.GetExtents3d("BlockExtents")
        Dim layoutId = row.GetObjectId("LayoutID")
        Dim width = extents.MaxPoint.X - extents.MinPoint.X
        Dim height = extents.MaxPoint.Y - extents.MinPoint.Y
        Dim center = New Point2d(extents.MinPoint.X + (width / 2), extents.MinPoint.Y + (height / 2))
        ViewHelper.SetToArea(document, layoutId, center, width, height)
        document.Editor.Regen()
        Dim image = ImageHelper.CropImageRelative(document.CapturePreviewImage(256, 256), width, height)
        XRecordImagesHelper.Save(document, "{Company}".Compose, "FramePics", row.GetString("Num"), image)
        XRecordHelper.Delete(document, "{Company}".Compose, "FramePics", document.FrameData.GetStringsFromColumn("Num"))
    End Sub

End Class
