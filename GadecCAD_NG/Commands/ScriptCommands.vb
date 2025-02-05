'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.Runtime

Public Module ScriptCommands

    ''' <summary>
    ''' The 'Renew Frames' command for use in a script.
    ''' <para>Note: Script can start from the filelist on the palette.</para>
    ''' </summary>
    <CommandMethod("DESIGNFRAMESSCRIPT")>
    Public Sub commandFramesScript()
        Try
            FrameInsertHelper.ReplaceFrames(ActiveDocument)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' Command to perform a step on the progressbar during a script.
    ''' </summary>
    <CommandMethod("PROGRESSBAR_PERFORMSTEP")>
    Public Sub commandProgressbarPerformStep()
        Try
            Progressbar?.PerformStep("{0}{1}".compose("File:".Translate, FileSystemHelper.LimitDisplayLengthFileName(ActiveDocument.Name, 60)))
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' Command to close the progressbar window at the end of a script.
    ''' </summary>
    <CommandMethod("PROGRESSBAR_DISPOSE")>
    Public Sub commandProgressbarHideAndClose()
        Try
            Progressbar?.Dispose()
            Progressbar = Nothing
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' Command to enable document events at the end of a script.
    ''' </summary>
    <CommandMethod("DOCEVENTSENABLE")>
    Public Sub commandDocEventsEnable()
        Try
            DocumentEvents.DocumentEventsEnabled = True
            PaletteHelper.ReloadFrameList()
            PaletteHelper.ReloadFileList()
            ScriptHelper.HelpDocument?.CloseAndDiscard
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' Command to disable document events at the start of a script.
    ''' </summary>
    <CommandMethod("DOCEVENTSDISABLE")>
    Public Sub commandDocEventsDisable()
        Try
            DocumentEvents.DocumentEventsEnabled = False
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

End Module
