'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports GadecCAD_NG.Extensions

''' <summary>
''' Provides methods for changing texts in the drawing.
''' </summary>
Public Class TextHelper

    ''' <summary>
    ''' Changes the text in textobjects with the specified objectids (key) to the specified value in the dictionary.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="textToChange">The dictionary containing textchanges for the attributes.</param>
    ''' <returns>A dictionary containing the previous texts of the attributes.</returns>
    Public Shared Function ChangeTextStrings(document As Document, textToChange As Dictionary(Of ObjectId, String)) As Dictionary(Of ObjectId, String)
        Using document.LockDocument
            Return ChangeTextStrings(document.Database, textToChange.ToArray)
        End Using
    End Function

    ''' <summary>
    ''' Changes the text in textobjects with the specified objectids (key) to the specified value in the dictionary.
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="textToChange">The dictionary containing textchanges for the attributes.</param>
    ''' <returns>A dictionary containing the previous texts of the attributes.</returns>
    Public Shared Function ChangeTextStrings(database As Database, textToChange As Dictionary(Of ObjectId, String)) As Dictionary(Of ObjectId, String)
        Return ChangeTextStrings(database, textToChange.ToArray)
    End Function

    ''' <summary>
    ''' Changes the text in textobjects with the specified objectids (key) to the specified value in the dictionary.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="textToChange">The dictionary containing textchanges for the attributes.</param>
    ''' <returns>A dictionary containing the previous texts of the attributes.</returns>
    Public Shared Function ChangeTextStrings(document As Document, textToChange As KeyValuePair(Of ObjectId, String)()) As Dictionary(Of ObjectId, String)
        Using document.LockDocument
            Return ChangeTextStrings(document.Database, textToChange)
        End Using
    End Function

    ''' <summary>
    ''' Changes the text in textobjects with the specified objectids (key) to the specified value in the dictionary.
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="textToChange">The dictionary containing textchanges for the attributes.</param>
    ''' <returns>A dictionary containing the previous texts of the attributes.</returns>
    Public Shared Function ChangeTextStrings(database As Database, textToChange As KeyValuePair(Of ObjectId, String)()) As Dictionary(Of ObjectId, String)
        Dim previousText = New Dictionary(Of ObjectId, String)
        If textToChange.Count = 0 Then Return previousText

        Dim currentDrawing = HostApplicationServices.WorkingDatabase
        HostApplicationServices.WorkingDatabase = database
        Dim pair = New KeyValuePair(Of ObjectId, String)
        Try
            Using tr = database.TransactionManager.StartTransaction
                For Each pair In textToChange
                    If pair.Key.IsNull Or pair.Key.IsErased Then Continue For

                    Dim entity = tr.GetObject(pair.Key, OpenMode.ForWrite, False, True)
                    If IsNothing(entity) Then Continue For

                    Select Case entity.GetDBObjectType
                        Case "DBText"
                            Dim dbText = entity.CastAsDBText
                            previousText.TryAdd(pair.Key, dbText.TextString)
                            dbText.TextString = pair.Value
                            dbText.AdjustAlignment(database)
                        Case "AttributeReference"
                            Dim attribute = entity.CastAsAttributeReference
                            previousText.TryAdd(pair.Key, attribute.TextString)
                            If attribute.Tag = "TYPE" AndAlso attribute.HasFields Then attribute.RemoveField()
                            attribute.TextString = pair.Value
                            attribute.AdjustAlignment(database)
                    End Select
                Next
                tr.Commit()
            End Using
        Catch ex As Exception
            ex.AddData($"ObjectId: {pair.Key}")
            ex.AddData($"String: {pair.Value}")
            ex.Rethrow
        End Try
        HostApplicationServices.WorkingDatabase = currentDrawing
        Return previousText
    End Function

End Class
