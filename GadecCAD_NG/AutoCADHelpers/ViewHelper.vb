'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.Geometry
Imports GadecCAD_NG.Extensions

''' <summary>
''' Provides methods for setting the view to a part of the drawing. 
''' </summary>
Public Class ViewHelper

    ''' <summary>
    ''' Sets the view of the drawing to the specified extents.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="layoutId">The objectid of the layout.</param>
    ''' <param name="extents">The extents of the view.</param>
    Public Shared Sub SetToExtents(document As Document, layoutId As ObjectId, extents As Extents3d)
        Dim ed = document.Editor
        Dim width = extents.MaxPoint.X - extents.MinPoint.X
        Dim height = extents.MaxPoint.Y - extents.MinPoint.Y
        Dim center = New Point2d(extents.MinPoint.X + (width / 2), extents.MinPoint.Y + (height / 2))
        SetToArea(document, layoutId, center, width, height)
    End Sub

    ''' <summary>
    ''' Sets the view of the drawing with the specified properties.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="layoutId">The objectid of the layout.</param>
    ''' <param name="center">The center for the view.</param>
    ''' <param name="width">The width of the view.</param>
    ''' <param name="height">The height of the view.</param>
    Public Shared Sub SetToArea(document As Document, layoutId As ObjectId, center As Point2d, width As Double, height As Double)
        Dim db = document.Database
        Dim ed = document.Editor
        Dim layoutName = LayoutHelper.SelectById(document, layoutId)
        If Not layoutName = "Model" Then ed.SwitchToPaperSpace()
        Using view = New ViewTableRecord With {.CenterPoint = center, .Height = height, .Width = width}
            view.IsPaperspaceView = Not db.ModelSpaceIsCurrent
            ed.SetCurrentView(view)
            ed.UpdateScreen()
        End Using
    End Sub

End Class
