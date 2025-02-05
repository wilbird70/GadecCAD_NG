'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.ApplicationServices
Imports GadecCAD_NG.Extensions

''' <summary>
''' Provides a method for executing a selected script.
''' <para>Scripts can be selected from the Scripts database in SetContextMenus.xml.</para>
''' </summary>
Public Class ScriptHelper
    ''' <summary>
    ''' The script starter uses a new document to start the created script.
    ''' </summary>
    ''' <returns>The new <see cref="Document"/></returns>
    Public Shared Property HelpDocument As Document

    ''' <summary>
    ''' Creates a script and starts it from a new temporary document.
    ''' <para>Note: Before executing the script, it closes the active document and any specified file if it is open.</para>
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="fileNames">The list of fullnames of files where the script should run.</param>
    ''' <param name="selectedScript">The name of the selected script.</param>
    Public Shared Sub Start(document As Document, fileNames As String(), selectedScript As String)
        Dim currentDocumentName = document.Name
        Dim scriptData = DataSetHelper.LoadFromXml("{Support}\SetContextMenus.xml".Compose).GetTable("Scripts", "Name")
        If NotNothing(scriptData) Then
            Dim row = scriptData.Rows.Find(selectedScript)
            If NotNothing(row) Then
                Dim documents = DocumentsHelper.GetOpenDocuments()
                Dim documentsToClose = New List(Of Document)
                Dim script = {"DOCEVENTSDISABLE"}.ToList
                Dim scriptString = row.GetString("Data")
                For Each fileName In fileNames
                    script.Add("OPEN {Q}{0}{Q} PROGRESSBAR_PERFORMSTEP {1} QSAVE CLOSE".Compose(fileName, scriptString))
                    If documents.ContainsKey(fileName) Then documentsToClose.Add(documents(fileName))
                Next
                script.Add("OPEN {Q}{0}{Q} PROGRESSBAR_DISPOSE DOCEVENTSENABLE".Compose(currentDocumentName))

                If Not documentsToClose.Contains(document) Then documentsToClose.Add(document)
                Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView()
                DocumentsHelper.Close(documentsToClose.ToArray, True)

                Dim scriptFile = "{AppDataFolder}\ScriptVarious.scr".Compose
                IO.File.WriteAllLines(scriptFile, script)
                Progressbar = New ProgressShow("Starting...".Translate, fileNames.Count, True)
                Try
                    _HelpDocument = DocumentsHelper.CreateNew()
                    _HelpDocument.SendString("(command {Q}_SCRIPT{Q} {Q}{0}{Q}) ".Compose(scriptFile.Replace("\", "\\")))
                Catch ex As System.Exception
                    Progressbar?.Dispose()
                    Progressbar = Nothing
                    ex.AddData($"Script: {selectedScript}")
                    ex.AddData($"{fileNames.Count} files")
                    ex.Rethrow
                End Try
            End If
        End If
    End Sub

End Class
