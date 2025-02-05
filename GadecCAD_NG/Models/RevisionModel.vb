'Gadec Engineerings Software (c) 2022
Imports System.Data

''' <summary>
''' <para><see cref="RevisionModel"/> contains a revisionlist database.</para>
''' </summary>
Public Class RevisionModel
    ''' <summary>
    ''' The indexnumber of the latest revision.
    ''' </summary>
    Private _lastRevisionNumber As String

    ''' <summary>
    ''' The revisionlist database.
    ''' </summary>
    Private ReadOnly _revisionTable As DataTable

    'class

    ''' <summary>
    ''' Initializes a new instance of <see cref="RevisionModel"/>.
    ''' <para><see cref="RevisionModel"/> contains a revisionlist database.</para>
    ''' </summary>
    Public Sub New()
        _revisionTable = EmptyRevisionList()
    End Sub

    'subs

    ''' <summary>
    ''' Adds a record to the revisionlist database with the specified data.
    ''' </summary>
    ''' <param name="revisionNumber">The index number of revision.</param>
    ''' <param name="revisionTag">The type of revision data.</param>
    ''' <param name="value">The revision data.</param>
    Public Sub AddRevisionData(revisionNumber As String, revisionTag As String, value As String)
        Dim row = _revisionTable.Rows.Find(revisionNumber)
        If IsNothing(row) Then
            row = _revisionTable.NewRow
            row("Rev") = revisionNumber
            _revisionTable.Rows.Add(row)
        End If
        row(revisionTag) = value
        If Not revisionTag = "Date" Then Exit Sub

        Dim revisionDate = DateStringHelper.Convert(value)
        Dim lastRevision = _revisionTable.Rows.Find(_lastRevisionNumber)
        Select Case True
            Case Not IsDate(revisionDate)
            Case IsNothing(lastRevision) : _lastRevisionNumber = revisionNumber
            Case Not IsDate(lastRevision.GetString("Date")) : _lastRevisionNumber = revisionNumber
            Case CDate(lastRevision.GetString("Date")) < CDate(revisionDate) : _lastRevisionNumber = revisionNumber
        End Select
    End Sub

    ''' <summary>
    ''' Copies the data of the latest revision to the framelist record.
    ''' </summary>
    ''' <param name="frameRow">The framelist record.</param>
    Public Sub CopyLastRevisionToDataRow(ByRef frameRow As DataRow)
        Dim lastRevision = _revisionTable.Rows.Find(_lastRevisionNumber)
        If IsNothing(lastRevision) Then
            lastRevision = _revisionTable.NewRow
            lastRevision("Char") = "0"
        End If
        frameRow("LastRev_Date") = lastRevision.GetString("Date")
        frameRow("LastRev_Char") = lastRevision.GetString("Char")
        frameRow("LastRev_Drawn") = lastRevision.GetString("Drawn")
        frameRow("LastRev_Descr") = lastRevision.GetString("Descr")
        frameRow("LastRev_Check") = lastRevision.GetString("Check")
        If lastRevision.GetString("KOPREV") = "" Then Exit Sub

        frameRow("LastRev_Char") = lastRevision.GetString("KOPREV").LeftString(1)
        Dim settings = " =;(=;)=".Cut.ToIniDictionary
        frameRow("LastRev_Drawn") = lastRevision.GetString("KOPREV").MidString(2).ReplaceMultiple(settings)
    End Sub

    'private functions

    ''' <summary>
    ''' Creates an revisionlist database with no records.
    ''' </summary>
    ''' <returns>The empty database.</returns>
    Private Function EmptyRevisionList() As DataTable
        Dim output = New DataTable("Revision")
        Dim columns = "Rev;Date;Char;Drawn;Descr;Check;KOPREV"
        output.InsertColumns(columns.Cut)
        output.AssignPrimaryKey("Rev")
        Return output
    End Function

End Class
