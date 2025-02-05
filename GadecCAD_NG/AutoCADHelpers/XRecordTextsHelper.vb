'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports GadecCAD_NG.Extensions

''' <summary>
''' Provides methods for loading and saving texts from/to the application's xrecords.
''' </summary>
Public Class XRecordTextsHelper

    'subs

    ''' <summary>
    ''' Saves texts to xrecords in the application's dictionary.
    ''' <para>Note: The keys of the dictionary become the entrynames of the xrecords.</para>
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="company">The name of the company.</param>
    ''' <param name="application">The name of the application.</param>
    ''' <param name="texts">The texts to store.</param>
    Public Shared Sub Save(document As Document, company As String, application As String, texts As Dictionary(Of String, String))
        Using document.LockDocument
            Save(document.Database, company, application, texts)
        End Using
    End Sub

    ''' <summary>
    ''' Saves texts to xrecords in the application's dictionary.
    ''' <para>Note: The keys of the dictionary become the entrynames of the xrecords.</para>
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="company">The name of the company.</param>
    ''' <param name="application">The name of the application.</param>
    ''' <param name="texts">The texts to store.</param>
    Public Shared Sub Save(database As Database, company As String, application As String, texts As Dictionary(Of String, String))
        Dim dictionaryId = XRecordHelper.GetDictionaryId(database, company, application, True)
        If dictionaryId.IsNull Then Exit Sub

        Using tr = database.TransactionManager.StartTransaction()
            Dim dictionary = tr.GetDBDictionary(dictionaryId, OpenMode.ForWrite)
            For Each pair In texts
                Dim resultBuffer = New ResultBuffer(New TypedValue(DxfCode.Text, pair.Value))
                Select Case dictionary.Contains(pair.Key)
                    Case True
                        Dim xRecord = tr.GetXrecord(dictionary.GetAt(pair.Key), OpenMode.ForWrite)
                        xRecord.XlateReferences = True
                        xRecord.Data = resultBuffer
                    Case Else
                        Dim xRecord = New Xrecord With {.XlateReferences = True, .Data = resultBuffer}
                        dictionary.SetAt(pair.Key, xRecord)
                        tr.AddNewlyCreatedDBObject(xRecord, True)
                End Select
            Next
            tr.Commit()
        End Using
    End Sub

    'functions

    ''' <summary>
    ''' Loads all texts from the xrecords in the application's dictionary.
    ''' <para>Note: The entrynames of the xrecords become the keys of the dictionary.</para>
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="company">The name of the company.</param>
    ''' <param name="application">The name of the application.</param>
    ''' <returns>The texts.</returns>
    Public Shared Function Load(fileName As String, company As String, application As String) As Dictionary(Of String, String)
        Dim output = New Dictionary(Of String, String)
        Dim documents = DocumentsHelper.GetOpenDocuments()
        Dim db As Database = Nothing
        Try
            Select Case True
                Case documents.ContainsKey(fileName)
                    db = documents(fileName).Database
                Case IO.File.Exists(fileName)
                    db = New Database(False, True)
                    db.ReadDwgFile(fileName, FileOpenMode.OpenForReadAndAllShare, True, "")
                Case Else : Return output
            End Select
            Dim dictionaryId = XRecordHelper.GetDictionaryId(db, company, application, False)
            If dictionaryId.IsNull Then Return output

            Using tr = db.TransactionManager.StartTransaction
                Dim dictionary = tr.GetDBDictionary(dictionaryId)
                For Each entry In dictionary
                    Dim xRecord = tr.GetXrecord(dictionary.GetAt(entry.Key))
                    Dim text = ""
                    For Each typedValue In xRecord.Data
                        text &= typedValue.Value
                    Next
                    output.TryAdd(entry.Key, text)
                Next
                tr.Commit()
            End Using
            Return output
        Catch ex As System.Exception
            Return output
        Finally
            db?.Dispose()
        End Try
    End Function

End Class
