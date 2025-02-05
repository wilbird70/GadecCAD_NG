'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.Runtime

Public Module RibbonTabFrames

    ''' <summary>
    ''' The 'Insert Frame' button.
    ''' <para>Frame insertion uses also the Gadec 'Design Center'.</para>
    ''' </summary>
    <CommandMethod("DESIGNCENTER2")>
    Public Sub commandDesignCenter2()
        Try
            DesignMethods.DesignCenter(ActiveDocument, "DCsessionFrames")
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' The 'Renew Frames' button.
    ''' </summary>
    <CommandMethod("DESIGNFRAMES")>
    Public Sub commandFrames()
        Try
            FrameInsertHelper.ReplaceFrames(ActiveDocument, True)
            PaletteHelper.ReloadFrameList()
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' The 'Set Scale' button.
    ''' </summary>
    <CommandMethod("SETSCALE")>
    Public Sub commandSetScale()
        Try
            DesignMethods.SetDrawingScale(ActiveDocument)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' The 'Frame Array' button.
    ''' </summary>
    <CommandMethod("DESIGNARRAY")>
    Public Sub commandArray()
        Try
            DesignMethods.DesignArray(ActiveDocument)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' The 'Polyline Frame' button.
    ''' </summary>
    <CommandMethod("PF")>
    Public Sub commandPolyFrame()
        Try
            FrameInsertHelper.ConvertPolylineToFrame(ActiveDocument)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

End Module
