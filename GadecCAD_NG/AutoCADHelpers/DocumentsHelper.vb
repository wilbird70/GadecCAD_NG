'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.ApplicationServices
Imports GadecCAD_NG.Extensions

''' <summary>
''' Provides methods for opening, closing and creating documents and more...
''' </summary>
Public Class DocumentsHelper

    'subs

    ''' <summary>
    ''' Closes the specified documents.
    ''' </summary>
    ''' <param name="documents">A list of documents.</param>
    ''' <param name="saveBeforeClose">If true, saves the documents first.</param>
    Public Shared Sub Close(documents As Document(), Optional saveBeforeClose As Boolean = False)
        For i = 0 To documents.Length - 1
            Progressbar?.SetText("Closing...".Translate(i + 1, documents.Length))
            Select Case saveBeforeClose
                Case True : documents(i)?.CloseAndSave(documents(i)?.Name)
                Case Else : documents(i)?.CloseAndDiscard()
            End Select
        Next
    End Sub

    ''' <summary>
    ''' Gets a list of filenames of the currently open documents.
    ''' </summary>
    ''' <returns>The list of filenames.</returns>
    Public Shared Function GetDocumentNames() As String()
        Return Application.DocumentManager.ToList.Select(Function(doc) doc.Name).ToArray
    End Function

    ''' <summary>
    ''' Gets a dictionary of the currently open documents (key=filename).
    ''' </summary>
    ''' <returns>The dictionary of documents.</returns>
    Public Shared Function GetOpenDocuments() As Dictionary(Of String, Document)
        Dim output = New Dictionary(Of String, Document)
        For Each document In Application.DocumentManager.ToArray
            document.WasClosed(False)
            output.TryAdd(document.Name, document)
        Next
        Return output
    End Function

    ''' <summary>
    ''' Opens the document with the specified filename.
    ''' </summary>
    ''' <param name="fileName">The filename of the document.</param>
    ''' <param name="showMessageIfCorrupt">If set to false, no corrupt message will be shown.</param>
    ''' <returns></returns>
    Public Shared Function Open(fileName As String, Optional showMessageIfCorrupt As Boolean = True) As Document
        Dim documents = GetOpenDocuments()
        Dim doc As Document = Nothing
        Dim documentWasClosed = True
        Select Case True
            Case documents.ContainsKey(fileName)
                doc = documents(fileName)
                Application.DocumentManager.MdiActiveDocument = doc
                documentWasClosed = False
            Case IO.File.Exists(fileName)
                Try
                    doc = Application.DocumentManager.Open(fileName, False)
                    Application.DocumentManager.MdiActiveDocument = doc
                Catch ex As Exception
                End Try
        End Select
        Select Case True
            Case NotNothing(doc)
                doc.WasClosed(documentWasClosed)
                Return doc
            Case Not showMessageIfCorrupt
                Return Nothing
            Case Not IO.File.Exists(fileName)
                MessageBoxInfo("FileNotFound".Translate(IO.Path.GetFileName(fileName)))
                Return Nothing
            Case Else
                MessageBoxInfo("Drawing error".Translate(IO.Path.GetFileName(fileName)))
                Return Nothing
        End Select
    End Function

    ''' <summary>
    ''' Creates a new document.
    ''' </summary>
    ''' <returns>The new document.</returns>
    Public Shared Function CreateNew() As Document
        Dim doc = DocumentCollectionExtension.Add(Application.DocumentManager, "acadiso.dwt")
        Application.DocumentManager.MdiActiveDocument = doc
        Return doc
    End Function

    ''' <summary>
    ''' Gets the information about a user when they would have the document in use.
    ''' </summary>
    ''' <param name="fileName">The filename of the document.</param>
    ''' <returns>A <see cref="UseModel"/> with the userinfo.</returns>
    Public Shared Function GetInuseInfo(fileName As String) As UseModel
        If fileName = "" Or IO.Path.GetDirectoryName(fileName) = "" Then Return Nothing

        Dim dwl = "{0}\{1}.dwl2".Compose(IO.Path.GetDirectoryName(fileName), IO.Path.GetFileNameWithoutExtension(fileName))
        If Not IO.File.Exists(dwl) OrElse Not FileSystemHelper.FileLocked(dwl) Then Return Nothing

        Try
            Using fileStream = New IO.FileStream(dwl, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.ReadWrite)
                Dim streamReader = New IO.StreamReader(fileStream)
                Dim text = streamReader.ReadToEnd
                Dim byOtherUser = Not GetAttribute(text, "username") = Environment.UserName
                Dim output = New UseModel(GetAttribute(text, "username"), GetAttribute(text, "machinename"), GetAttribute(text, "datetime"), byOtherUser)
                streamReader.Close()
                Return output
            End Using
        Catch ex As System.Exception
            Return Nothing
        End Try
    End Function

    'private functions

    ''' <summary>
    ''' Gets the value of an attribute out of a xml-string.
    ''' </summary>
    ''' <param name="xmlString">The xml-string.</param>
    ''' <param name="attribute">The name of the attribute.</param>
    ''' <returns></returns>
    Private Shared Function GetAttribute(xmlString As String, attribute As String) As String
        Return xmlString.InStrResult("<{0}>".Compose(attribute), "</{0}>".Compose(attribute))
    End Function

End Class
