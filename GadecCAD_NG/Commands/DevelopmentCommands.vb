'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.Runtime
Imports GadecCAD_NG.Extensions

Public Module DevelopmentCommands

    ''' <summary>
    ''' Developers command to create the thumbnails images from blockreferences in Modelspace.
    ''' <para>This command often has to be run twice, the first time set the windowsize.</para>
    ''' <para>Note: These images are used in the Gadec 'Design Center'.</para>
    ''' </summary>
    <CommandMethod("CREATEBLOCKIMAGES")>
    Public Sub commandCreateBlockImages()
        Try
            BlockPreviews.Create(ActiveDocument)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' Developers command to export all the blockdefinitions to the appdata folder in separate drawingfiles.
    ''' </summary>
    <CommandMethod("EXPBLKDEF")>
    Public Sub commandExportBlockDefinitions()
        Try
            DefinitionMethod.ExportAll(ActiveDocument)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' Developers command to clear a drawing.
    ''' </summary>
    <CommandMethod("EA")>
    Public Sub commandEraseAll()
        Try
            ActiveDocument.SendString("(command {Q}ERASE{Q} {Q}ALL{Q} {Q}{Q}) ".compose)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' Developers command to clear a drawing.
    ''' </summary>
    <CommandMethod("TOWNG")>
    Public Sub commandToWng()
        Try
            NotUsed.ReplaceBlocksOld2Wng(ActiveDocument)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

End Module
