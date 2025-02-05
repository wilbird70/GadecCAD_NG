'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports GadecCAD_NG.Extensions

''' <summary>
''' Provides methods for loading, saving and updating objectidcollections from/to the application's xrecords.
''' </summary>
Public Class XRecordObjectIdsHelper

    'subs

    ''' <summary>
    ''' Saves the objectidcollections to xrecords in the application's dictionary and deletes all other xrecords.
    ''' <para>Note: The keys of the dictionary become the entrynames of the xrecords.</para>
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="company">The name of the company.</param>
    ''' <param name="application">The name of the application.</param>
    ''' <param name="objectIdCollections">The objectidcollections to store.</param>
    Public Shared Sub Update(document As Document, company As String, application As String, objectIdCollections As Dictionary(Of String, ObjectIdCollection))
        Using document.LockDocument
            Update(document.Database, company, application, objectIdCollections)
        End Using
    End Sub

    ''' <summary>
    ''' Saves the objectidcollections to xrecords in the application's dictionary and deletes all other xrecords.
    ''' <para>Note: The keys of the dictionary become the entrynames of the xrecords.</para>
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="company">The name of the company.</param>
    ''' <param name="application">The name of the application.</param>
    ''' <param name="objectIdCollections">The objectidcollections to store.</param>
    Public Shared Sub Update(database As Database, company As String, application As String, objectIdCollections As Dictionary(Of String, ObjectIdCollection))
        If objectIdCollections.Count = 0 Then XRecordHelper.Delete(database, company, application) : Exit Sub

        Save(database, company, application, objectIdCollections)
        XRecordHelper.Delete(database, company, application, objectIdCollections.Keys.ToArray)
    End Sub

    ''' <summary>
    ''' Saves objectidcollections to xrecords in the application's dictionary.
    ''' <para>Note: The keys of the dictionary become the entrynames of the xrecords.</para>
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="company">The name of the company.</param>
    ''' <param name="application">The name of the application.</param>
    ''' <param name="objectIdCollections">The objectidcollections to store.</param>
    Public Shared Sub Save(database As Database, company As String, application As String, objectIdCollections As Dictionary(Of String, ObjectIdCollection))
        Dim dictionaryId = XRecordHelper.GetDictionaryId(database, company, application, True)
        If dictionaryId.IsNull Then Exit Sub

        Using tr = database.TransactionManager.StartTransaction()
            Dim dictionary = tr.GetDBDictionary(dictionaryId, OpenMode.ForWrite)
            For Each pair In objectIdCollections
                Dim resultBuffer = New ResultBuffer
                For i = 0 To pair.Value.Count - 1
                    resultBuffer.Add(New TypedValue(DxfCode.SoftPointerId + i, pair.Value(i)))
                Next
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
    ''' Loads the objectidcollection from the xrecord in the application's dictionary on the specified entryname.
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="company">The name of the company.</param>
    ''' <param name="application">The name of the application.</param>
    ''' <param name="entryName">The entryname of the xrecord.</param>
    ''' <returns>The objectidcollection.</returns>
    Public Shared Function Load(database As Database, company As String, application As String, entryName As String) As ObjectIdCollection
        Dim dictionaryId = XRecordHelper.GetDictionaryId(database, company, application, False)
        If dictionaryId.IsNull Then Return New ObjectIdCollection

        Dim output = New ObjectIdCollection
        Using tr = database.TransactionManager.StartTransaction
            Dim dictionary = tr.GetDBDictionary(dictionaryId)
            If Not dictionary.Contains(entryName) Then entryName = "Frame_{0}".Compose(entryName) 'for legacy purposes.
            If dictionary.Contains(entryName) Then
                Dim xRecord = tr.GetXrecord(dictionary.GetAt(entryName))
                For Each typedValue In xRecord.Data
                    output.Add(typedValue.Value)
                Next
            End If
            tr.Commit()
        End Using
        Return output
    End Function

    ''' <summary>
    ''' Loads all objectidcollections from the xrecords in the application's dictionary.
    ''' <para>Note: The entrynames of the xrecords become the keys of the dictionary.</para>
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="company">The name of the company.</param>
    ''' <param name="application">The name of the application.</param>
    ''' <returns>The objectidcollections.</returns>
    Public Shared Function Load(database As Database, company As String, application As String) As Dictionary(Of String, ObjectIdCollection)
        Dim dictionaryId = XRecordHelper.GetDictionaryId(database, company, application, False)
        If dictionaryId.IsNull Then Return New Dictionary(Of String, ObjectIdCollection)

        Dim output = New Dictionary(Of String, ObjectIdCollection)
        Using tr = database.TransactionManager.StartTransaction
            Dim dictionary = tr.GetDBDictionary(dictionaryId)
            For Each entry In dictionary
                Dim xRecord = tr.GetXrecord(dictionary.GetAt(entry.Key))
                Dim frameIDs = New ObjectIdCollection
                For Each typedValue In xRecord.Data
                    frameIDs.Add(typedValue.Value)
                Next
                output.TryAdd(entry.Key, frameIDs)
            Next
            tr.Commit()
        End Using
        Return output
    End Function

End Class
