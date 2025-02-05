'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.Runtime
Imports GadecCAD_NG.Extensions

Public Module ExtraCommands

    ''' <summary>
    ''' Replaces the Stretch command shotcut with one that directly uses the crossing option. 
    ''' </summary>
    <CommandMethod("S")>
    Public Sub commandStretchCrossing()
        Try
            ActiveDocument.SendString("(command {Q}STRETCH{Q} {Q}C{Q} pause) ".compose)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' Command to join connecting lines and arcs together in a polyline. 
    ''' </summary>
    <CommandMethod("JP")>
    Public Sub commandJoinPolylines()
        Try
            PolylineMethods.Join(ActiveDocument)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' Command to send all the wipeouts in a block to the bottom to prevent them to cover entities.
    ''' </summary>
    <CommandMethod("WTB")>
    Public Sub commandWipeoutsToBottom()
        Try
            Dim referenceIds = SelectMethods.GetSelectionOfReferences(ActiveDocument)

            DefinitionHelper.SendToBackWipeouts(ActiveDocument.Database, referenceIds)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    'disabled

    ''' <summary>
    ''' Command to convert symbols from ThornGraphic to Winguard.
    ''' <para>Note: This command is currently disabled.</para>
    ''' </summary>
    <CommandMethod("TGX2WNG")>
    Public Sub commandTgx2Wng()
        Try
            'sReplaceBlocksTgx2Wng(fActiveDocument)
            MsgBox("Function disabled")
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

End Module
