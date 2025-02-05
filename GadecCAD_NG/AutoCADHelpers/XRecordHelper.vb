'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports GadecCAD_NG.Extensions

''' <summary>
''' Provides common methods for the drawings xrecords.
''' </summary>
Public Class XRecordHelper

    'subs

    ''' <summary>
    ''' Deletes xrecords from the application dictionary, excepts those with the specified keys.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="company">The name of the company.</param>
    ''' <param name="application">The name of the application.</param>
    ''' <param name="excepts">A list of keys not to delete.</param>
    Public Shared Sub Delete(document As Document, company As String, application As String, excepts As String())
        Using document.LockDocument
            Delete(document.Database, company, application, excepts)
        End Using
    End Sub

    ''' <summary>
    ''' Deletes xrecords from the application dictionary except those with the specified keys.
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="company">The name of the company.</param>
    ''' <param name="application">The name of the application.</param>
    ''' <param name="excepts">A list of keys not to delete.</param>
    Public Shared Sub Delete(database As Database, company As String, application As String, excepts As String())
        Dim dictionaryId = XRecordHelper.GetDictionaryId(database, company, application, False)
        If dictionaryId.IsNull Then Exit Sub

        Using tr = database.TransactionManager.StartTransaction()
            Dim dictionary = tr.GetDBDictionary(dictionaryId, OpenMode.ForWrite)
            For Each entry In dictionary
                If Not excepts.Contains(entry.Key) Then tr.GetXrecord(dictionary.GetAt(entry.Key), OpenMode.ForWrite).Erase()
            Next
            tr.Commit()
        End Using
    End Sub

    ''' <summary>
    ''' Deletes the application dictionary.
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="company">The name of the company.</param>
    ''' <param name="application">The name of the application.</param>
    Public Shared Sub Delete(database As Database, company As String, application As String)
        Using tr = database.TransactionManager.StartTransaction()
            Dim namedObjectsDictionary = tr.GetDBDictionary(database.NamedObjectsDictionaryId)
            If Not namedObjectsDictionary.Contains(company) Then Exit Sub

            Dim companyDictionary = tr.GetDBDictionary(namedObjectsDictionary.GetAt(company), OpenMode.ForWrite)
            If companyDictionary.Contains(application) Then
                companyDictionary.UpgradeOpen()
                companyDictionary.Remove(companyDictionary.GetAt(application))
            End If
            If companyDictionary.Count = 0 Then
                namedObjectsDictionary.UpgradeOpen()
                namedObjectsDictionary.Remove(namedObjectsDictionary.GetAt(company))
            End If
            tr.Commit()
        End Using
    End Sub

    'functions

    ''' <summary>
    ''' Gets the objectid from the application dictionary within the named object dictionary.
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="company">The name of the company.</param>
    ''' <param name="application">The name of the application.</param>
    ''' <param name="createIfNotExisting">If true, a new dictionary will be created if it does not exist.</param>
    ''' <returns>The objectid.</returns>
    Public Shared Function GetDictionaryId(database As Database, company As String, application As String, createIfNotExisting As Boolean) As ObjectId
        Dim output = ObjectId.Null
        If IsNothing(database?.TransactionManager) Then Return output

        Using tr = database.TransactionManager.StartTransaction()
            Dim namedObjectsDictionary = tr.GetDBDictionary(database.NamedObjectsDictionaryId)
            Select Case True
                Case namedObjectsDictionary.Contains(company)
                    Dim companyDictionary = tr.GetDBDictionary(namedObjectsDictionary.GetAt(company))
                    Select Case True
                        Case companyDictionary.Contains(application)
                            output = companyDictionary.GetAt(application)
                        Case createIfNotExisting
                            Dim applicationDictionary = New DBDictionary()
                            companyDictionary.UpgradeOpen()
                            output = companyDictionary.SetAt(application, applicationDictionary)
                            tr.AddNewlyCreatedDBObject(applicationDictionary, True)
                    End Select
                Case createIfNotExisting
                    Dim cmpDic = New DBDictionary()
                    namedObjectsDictionary.UpgradeOpen()
                    namedObjectsDictionary.SetAt(company, cmpDic)
                    tr.AddNewlyCreatedDBObject(cmpDic, True)
                    Dim appDic = New DBDictionary()
                    output = cmpDic.SetAt(application, appDic)
                    tr.AddNewlyCreatedDBObject(appDic, True)
            End Select
            tr.Commit()
        End Using
        Return output
    End Function

End Class
