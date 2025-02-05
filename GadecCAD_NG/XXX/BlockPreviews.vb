'Gadec Engineerings Software (c) 2022
Imports System.Drawing
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.Geometry
Imports GadecCAD_NG.Extensions

''' <summary>
''' Provides a method to create Gadec blockpreviews.
''' </summary>
Public Class BlockPreviews

    'subs

    ''' <summary>
    ''' Creates Gadec blockpreviews from all the blockreferences found in modelspace.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    Public Shared Sub Create(document As Document)
        Dim db = document.Database
        Dim ed = document.Editor
        Dim layoutId = LayoutHelper.GetIdByName(db, "Model")
        SetWindow(document, System.Windows.WindowState.Normal, New System.Windows.Point(0, 0), New System.Windows.Size(1500, 900))
        ed.Command("-PURGE", "ALL", "*", "N")

        XRecordHelper.Delete(document.Database, "{Company}".Compose, "BlockPics")
        Dim color = Application.Preferences.Display.GraphicsWinModelBackgrndColor
        Try
            Application.Preferences.Display.GraphicsWinModelBackgrndColor = Drawing.Color.Black
            Dim images = New Dictionary(Of String, Drawing.Bitmap)
            Dim ids = SelectHelper.GetAllReferencesInModelspace(document)
            Dim referenceData = ReferenceHelper.GetReferenceData(document.Database, ids)
            For Each row In referenceData.Select
                Dim referenceImage = CapturePreviewImage(document, layoutId, row.GetExtents3d("BlockExtents"))
                images.TryAdd(row.GetString("BlockName"), referenceImage)
            Next
            XRecordImagesHelper.Save(document, "{Company}".Compose, "BlockPics", images)
            XRecordHelper.Delete(document, "{Company}".Compose, "BlockPics", images.Keys.ToArray)
        Catch ex As System.Exception
            ex.Rethrow
        Finally
            Application.Preferences.Display.GraphicsWinModelBackgrndColor = color
            SetWindow(document, System.Windows.WindowState.Maximized)
        End Try
        ed.Command("ZOOM", "E")
    End Sub

    'private subs

    ''' <summary>
    ''' Captures the preview image with the specified extents on the specified layout.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="layoutId">The objectid of the layout.</param>
    ''' <param name="extents">The extents of the area to capture.</param>
    Private Shared Function CapturePreviewImage(document As Document, layoutId As ObjectId, extents As Extents3d) As Bitmap
        Dim width = extents.MaxPoint.X - extents.MinPoint.X
        Dim height = extents.MaxPoint.Y - extents.MinPoint.Y
        Dim center = New Point2d(extents.MinPoint.X + (width / 2), extents.MinPoint.Y + (height / 2))
        width = {28, width * 3.2}.Max
        height = {28, height * 2.8}.Max
        ViewHelper.SetToArea(document, layoutId, center, width, height)
        Dim bitmap = document.CapturePreviewImage(256, 256)
        Dim output = ImageHelper.CropImage(bitmap, 96, 64)
        Return output
    End Function

    ''' <summary>
    ''' Sets the dwgview window of the document to the specified state, location and size.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="state">The state of the window (eg. normal or maximized).</param>
    ''' <param name="location">The location of the window (by normal-state window).</param>
    ''' <param name="size">The size of the window (by normal-state window).</param>
    Private Shared Sub SetWindow(document As Document, state As System.Windows.WindowState, Optional location As System.Windows.Point = Nothing, Optional size As System.Windows.Size = Nothing)
        document.Window.WindowState = state
        If state = System.Windows.WindowState.Normal Then
            document.Window.DeviceIndependentLocation = If(NotNothing(location), location, New System.Windows.Point(0, 0))
            document.Window.DeviceIndependentSize = If(NotNothing(size), size, New System.Windows.Size(100, 100))
        End If
    End Sub

    'functions

    ''' <summary>
    ''' Determine if dwgview window is set to the specified size.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="size">The requested size of the window.</param>
    ''' <returns>True if size matches the  dwgview window size.</returns>
    Private Shared Function WindowIsSet(document As Document, size As System.Windows.Size) As Boolean
        Select Case True
            Case Not document.Window.WindowState = System.Windows.WindowState.Normal : Return False
            Case Not document.Window.DeviceIndependentSize = size : Return False
            Case Else : Return True
        End Select
    End Function

End Class
