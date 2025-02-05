'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.Runtime

Public Module RibbonTabLines

    ''' <summary>
    ''' The 'Assign Xlinks' button.
    ''' </summary>
    <CommandMethod("XLINKS")>
    Public Sub commandXlinks()
        Try
            Dim xlinkshandler = New XlinksHandler(ActiveDocument)
            xlinkshandler.Start()
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' The 'Cutting Gapline' button.
    ''' </summary>
    <CommandMethod("GLC")>
    Public Sub commandGapLineCutting()
        Try
            GapLineMethods.Cut(ActiveDocument)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' The 'Drawing Gapline' button.
    ''' </summary>
    <CommandMethod("GL")>
    Public Sub commandGapLine()
        Try
            GapLineMethods.Draw(ActiveDocument)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' The 'Join Lines' button.
    ''' </summary>
    <CommandMethod("JL")>
    Public Sub commandJoinLines()
        Try
            LineMethods.Join(ActiveDocument)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' The 'Zonal Line' button.
    ''' </summary>
    <CommandMethod("ZL")>
    Public Sub commandZonePolyline()
        Try
            ZoneLineMethod.Draw(ActiveDocument)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' The 'Loop Line' button.
    ''' </summary>
    <CommandMethod("LL")>
    Public Sub commandLoopLine()
        Try
            Dim layerId = LayerHelper.GetLayerIdFromType(ActiveDocument.Database, "Extra")
            LoopLineMethods.DrawLoopLine(ActiveDocument, layerId)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' The 'Remote Line' button.
    ''' </summary>
    <CommandMethod("RL")>
    Public Sub commandRemoteLEDLine()
        Try
            Dim layerId = LayerHelper.GetLayerIdFromType(ActiveDocument.Database, "Line")
            LoopLineMethods.DrawRemoteLine(ActiveDocument, layerId)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' The 'Snap 1.25' button.
    ''' </summary>
    <CommandMethod("SNP")>
    Public Sub commandSnap125()
        Try
            SnapHelper.Set125(ActiveDocument)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' The 'Snap Halve' button.
    ''' </summary>
    <CommandMethod("SNH")>
    Public Sub commandSnapHalve()
        Try
            SnapHelper.HalveIt(ActiveDocument)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' The 'Snap Double' button.
    ''' </summary>
    <CommandMethod("SND")>
    Public Sub commandSnapDouble()
        Try
            SnapHelper.DoubleIt(ActiveDocument)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

End Module
