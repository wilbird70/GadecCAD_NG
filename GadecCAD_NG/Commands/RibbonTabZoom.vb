'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.Runtime
Imports GadecCAD_NG.Extensions

Public Module RibbonTabZoom

    ''' <summary>
    ''' The 'Zoom Extents' button.
    ''' </summary>
    <CommandMethod("ZE", CommandFlags.NoUndoMarker)>
    Public Sub commandZoomExtents()
        Try
            ActiveDocument.ZoomExtents
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' The 'Zoom Window' button.
    ''' </summary>
    <CommandMethod("ZW")>
    Public Sub commandZoomWindow()
        Try
            ActiveDocument.SendString("(command {Q}ZOOM{Q} {Q}W{Q} pause pause) ".Compose)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' The 'Zoom Previous' button.
    ''' </summary>
    <CommandMethod("ZP")>
    Public Sub commandZoomPrevious()
        Try
            ActiveDocument.SendString("(command {Q}ZOOM{Q} {Q}P{Q}) ".Compose)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

End Module
