'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.ApplicationServices
Imports System.Runtime.CompilerServices

Namespace Extensions

    Public Module DocumentExtensions

        ''' <summary>
        ''' Creates (if not exists) the <see cref="GadecCAD.DocumentEvents"/> for the document.
        ''' </summary>
        ''' <param name="eDocument"></param>
        ''' <returns>The <see cref="GadecCAD.DocumentEvents"/>.</returns>
        <Extension()>
        Public Function DocumentEvents(ByVal eDocument As Document) As DocumentEvents
            Select Case eDocument.UserData.ContainsKey("DocumentEvents")
                Case True : Return eDocument.UserData("DocumentEvents")
                Case Else
                    Dim output = New DocumentEvents(eDocument)
                    eDocument.UserData.Add("DocumentEvents", output)
                    Return output
            End Select
        End Function

        ''' <summary>
        ''' Creates (if not exists) or refreshes the frame database for the document.
        ''' </summary>
        ''' <param name="eDocument"></param>
        ''' <param name="refresh">Optional to refresh the database.</param>
        ''' <returns>The frame database.</returns>
        <Extension()>
        Public Function FrameData(ByVal eDocument As Document, Optional refresh As Boolean = False) As Data.DataTable
            If eDocument.UserData.ContainsKey("FrameTable") And Not refresh Then Return eDocument.UserData("FrameTable")

            Dim output = FrameHelper.BuildFrameData(eDocument, FrameFindHelper.AllFrames(eDocument))
            Select Case eDocument.UserData.ContainsKey("FrameTable")
                Case True : eDocument.UserData("FrameTable") = output
                Case Else : eDocument.UserData.Add("FrameTable", output)
            End Select
            Return output
        End Function

        ''' <summary>
        ''' Gets or sets the active frame for the document.
        ''' </summary>
        ''' <param name="eDocument"></param>
        ''' <param name="newValue">Unique string of the frame.</param>
        ''' <returns>Unique string of the frame.</returns>
        <Extension()>
        Public Function ActiveFrame(ByVal eDocument As Document, Optional newValue As String = "") As String
            Select Case True
                Case newValue = ""
                Case eDocument.UserData.ContainsKey("ActiveFrame") : eDocument.UserData("ActiveFrame") = newValue
                Case Else : eDocument.UserData.Add("ActiveFrame", newValue)
            End Select
            Select Case eDocument.UserData.ContainsKey("ActiveFrame")
                Case True : Return eDocument.UserData("ActiveFrame")
                Case Else : Return ""
            End Select
        End Function

        ''' <summary>
        ''' Gets or sets the 'WasClosed' <see cref="Boolean"/> for the document.
        ''' </summary>
        ''' <param name="eDocument"></param>
        ''' <param name="newValue">Sets the new value for whatever the document was closed or not.</param>
        ''' <returns>The <see cref="Boolean"/> value.</returns>
        <Extension()>
        Public Function WasClosed(ByVal eDocument As Document, Optional newValue As Nullable(Of Boolean) = Nothing) As Boolean
            Select Case True
                Case IsNothing(newValue)
                Case eDocument.UserData.ContainsKey("WasClosed") : eDocument.UserData("WasClosed") = newValue
                Case Else : eDocument.UserData.Add("WasClosed", newValue)
            End Select
            Select Case eDocument.UserData.ContainsKey("WasClosed")
                Case True : Return eDocument.UserData("WasClosed")
                Case Else : Return False
            End Select
        End Function

        ''' <summary>
        ''' Gets or sets the 'EditorNeedsRegen' <see cref="Boolean"/> for the document.
        ''' </summary>
        ''' <param name="eDocument"></param>
        ''' <param name="newValue">Sets the new value for whatever the document needs a Regen or not.</param>
        ''' <returns>The <see cref="Boolean"/> value.</returns>
        <Extension()>
        Public Function EditorNeedsRegen(ByVal eDocument As Document, Optional newValue As Nullable(Of Boolean) = Nothing) As Boolean
            Select Case True
                Case IsNothing(newValue)
                Case eDocument.UserData.ContainsKey("EditorNeedsRegen") : eDocument.UserData("EditorNeedsRegen") = newValue
                Case Else : eDocument.UserData.Add("EditorNeedsRegen", newValue)
            End Select
            Select Case eDocument.UserData.ContainsKey("EditorNeedsRegen")
                Case True : Return eDocument.UserData("EditorNeedsRegen")
                Case Else : Return False
            End Select
        End Function

        ''' <summary>
        ''' Gets a counting number for inserting frames from polylines in the document.
        ''' </summary>
        ''' <param name="eDocument"></param>
        ''' <returns>An <see cref="Integer"/> value.</returns>
        <Extension()>
        Public Function AutoNumber(ByVal eDocument As Document) As Integer
            Select Case eDocument.UserData.ContainsKey("SheetNumber")
                Case True : eDocument.UserData("SheetNumber") = eDocument.UserData("SheetNumber") + 1 : Return eDocument.UserData("SheetNumber")
                Case Else : eDocument.UserData.Add("SheetNumber", 1) : Return 1
            End Select
        End Function

        ''' <summary>
        ''' Gets the name of the folder where the doument is saved.
        ''' </summary>
        ''' <param name="eDocument"></param>
        ''' <returns>The foldername.</returns>
        <Extension()>
        Public Function GetPath(ByVal eDocument As Document) As String
            Return IO.Path.GetDirectoryName(eDocument?.Name)
        End Function

        ''' <summary>
        ''' Gets the name of the file without the folder path.
        ''' </summary>
        ''' <param name="eDocument"></param>
        ''' <returns>The filename.</returns>
        <Extension()>
        Public Function GetFileName(ByVal eDocument As Document) As String
            Return IO.Path.GetFileName(eDocument?.Name)
        End Function

        ''' <summary>
        ''' Gets the name of the file without the folder path and extension.
        ''' </summary>
        ''' <param name="eDocument"></param>
        ''' <returns>The filename without extension.</returns>
        <Extension()>
        Public Function GetFileNameWithoutExtension(ByVal eDocument As Document) As String
            Return IO.Path.GetFileNameWithoutExtension(eDocument?.Name)
        End Function

        ''' <summary>
        ''' Gets whatever the drawing is saved or not.
        ''' </summary>
        ''' <param name="eDocument"></param>
        ''' <returns>True if drawing is not saved.</returns>
        <Extension()>
        Public Function NotNamedDrawing(ByVal eDocument As Document) As Boolean
            Return Not eDocument?.IsNamedDrawing
        End Function

        ''' <summary>
        ''' Sends a string to the commandline to execute.
        ''' </summary>
        ''' <param name="eDocument"></param>
        ''' <param name="command">The commandstring.</param>
        <Extension()>
        Public Sub SendString(ByVal eDocument As Document, command As String)
            Try
                eDocument.SendStringToExecute(command, True, False, True)
            Catch ex As Exception
                ex.AddData($"Command: {command}")
                ex.ReThrow
            End Try
        End Sub

        ''' <summary>
        ''' Sets the focus to the drawing area and zooms to the extents.
        ''' </summary>
        ''' <param name="eDocument"></param>
        <Extension()>
        Public Sub ZoomExtents(ByVal eDocument As Document)
            Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView()
            If NotNothing(eDocument) Then eDocument.SendString("(command {Q}ZOOM{Q} {Q}E{Q}) ".Compose)
        End Sub

        ''' <summary>
        ''' Cancels the last active command.
        ''' </summary>
        ''' <param name="eDocument"></param>
        <Extension()>
        Public Sub CancelCommand(ByVal eDocument As Document)
            Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView()
            If NotNothing(eDocument) Then eDocument.SendString("{Esc}{Esc}".Compose)
        End Sub

    End Module

End Namespace
