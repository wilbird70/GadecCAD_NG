'Gadec Engineerings Software (c) 2022
Imports System.Data

''' <summary>
''' <para><see cref="FrameSetModel"/> creates and contains a frame- and filelist database collection.</para>
''' </summary>
Public Class FrameSetModel
    ''' <summary>
    ''' The framelist database read from the projects drawinglist.xml-file.
    ''' </summary>
    ''' <returns>The database.</returns>
    Public ReadOnly Property FrameListData As DataTable
    ''' <summary>
    ''' The filelist database read from the projects drawinglist.xml-file.
    ''' </summary>
    ''' <returns>The database.</returns>
    Public ReadOnly Property FileListData As DataTable
    ''' <summary>
    ''' The updated framelist database (with current data from unsaved open documents).
    ''' <para>This database belongs to a database collection that also contains the filelist database.</para>
    ''' </summary>
    ''' <returns>The database.</returns>
    Public ReadOnly Property UpdatedFrameListData As DataTable = Nothing

    ''' <summary>
    ''' The frame- and filelist database collection read from the projects drawinglist.xml-file.
    ''' </summary>
    Private ReadOnly _frameSet As DataSet
    ''' <summary>
    ''' The fullname of the drawinglist.xml-file for the project.
    ''' </summary>
    Private ReadOnly _xmlFileName As String
    ''' <summary>
    ''' A list of framelist records to built a database which will be saved in the projects drawinglist.xml-file.
    ''' </summary>
    Private ReadOnly _newXmlFrameRows As New List(Of DataRow)
    ''' <summary>
    ''' A list of framelist records to built a database which will be displayed on the frame-palette.
    ''' </summary>
    Private ReadOnly _actualFrameRows As New List(Of DataRow)
    ''' <summary>
    ''' A list of filelist records to built a database which will be saved in the projects drawinglist.xml-file.
    ''' </summary>
    Private ReadOnly _newXmlFileRows As New List(Of DataRow)
    ''' <summary>
    ''' A list of filelist records to built a database which will be displayed on the frame-palette (frameless section).
    ''' </summary>
    Private ReadOnly _actualFileRows As New List(Of DataRow)

    'class

    ''' <summary>
    ''' Initializes a new instance of <see cref="FrameSetModel"/>.
    ''' <para><see cref="FrameSetModel"/> creates and contains a frame- and filelist database collection.</para>
    ''' </summary>
    ''' <param name="xmlFileName">The fullname of the drawinglist.xml-file for the project (folder).</param>
    Public Sub New(xmlFileName As String)
        _xmlFileName = xmlFileName
        Try
            _frameSet = DataSetHelper.LoadFromXml(xmlFileName)
        Catch ex As Exception
            _frameSet = New DataSet("GadecAutoCAD")
        End Try

        If Not _frameSet.Tables.Contains("Frames") Then _frameSet.Tables.Add(EmptyFrameList)
        If Not _frameSet.Tables.Contains("Files") Then _frameSet.Tables.Add(EmptyFileList)
        _FrameListData = _frameSet.Tables("Frames")
        _FrameListData.Merge(EmptyFrameList)
        _FileListData = _frameSet.Tables("Files")
        _FileListData.Merge(EmptyFileList)
    End Sub

    'subs

    ''' <summary>
    ''' Adds the records to the framelist records to built a database which will be saved in the projects drawinglist.xml-file.
    ''' </summary>
    ''' <param name="rows"></param>
    Public Sub AddToSavedFrameList(rows As DataRow())
        _newXmlFrameRows.AddRange(rows)
    End Sub

    ''' <summary>
    ''' Adds the records to the framelist records to built a database which will be displayed on the frame-palette.
    ''' </summary>
    ''' <param name="rows"></param>
    Public Sub AddToActualFrameList(rows As DataRow())
        _actualFrameRows.AddRange(rows)
    End Sub

    ''' <summary>
    ''' Adds the records to a filelist record to built a database which will be saved in the projects drawinglist.xml-file.
    ''' </summary>
    ''' <param name="row"></param>
    Public Sub AddToSavedFileList(row As DataRow)
        _newXmlFileRows.Add(row)
    End Sub

    ''' <summary>
    ''' Adds the records to a filelist record to built a database which will be displayed on the frame-palette (frameless section).
    ''' </summary>
    ''' <param name="row"></param>
    Public Sub AddToActualFileList(row As DataRow)
        _actualFileRows.Add(row)
    End Sub

    ''' <summary>
    ''' Saves the frame- and filelist database collection to the drawinglist.xml-file for the project.
    ''' </summary>
    Public Sub Save()
        Dim actualDataSet = New DataSet("GadecAutoCAD")
        Dim newXmlDataSet = New DataSet("GadecAutoCAD")

        Select Case _actualFrameRows.Count = 0
            Case True : actualDataSet.Tables.Add(EmptyFrameList)
            Case Else
                Dim frameListData = _actualFrameRows.CopyToDataTable()
                frameListData.TableName = "Frames"
                frameListData.MapColumnsToAttribute
                actualDataSet.Tables.Add(frameListData)
        End Select
        Select Case _actualFileRows.Count = 0
            Case True : actualDataSet.Tables.Add(EmptyFileList)
            Case Else
                Dim fileListData = _actualFileRows.CopyToDataTable()
                fileListData.TableName = "Files"
                fileListData.MapColumnsToAttribute
                actualDataSet.Tables.Add(fileListData)
        End Select
        Select Case _newXmlFrameRows.Count = 0
            Case True : newXmlDataSet.Tables.Add(EmptyFrameList)
            Case Else
                Dim frameListData = _newXmlFrameRows.CopyToDataTable()
                frameListData.TableName = "Frames"
                frameListData.MapColumnsToAttribute
                newXmlDataSet.Tables.Add(frameListData)
        End Select
        Select Case _newXmlFileRows.Count = 0
            Case True : newXmlDataSet.Tables.Add(EmptyFileList)
            Case Else
                Dim fileListData = _newXmlFileRows.CopyToDataTable()
                fileListData.TableName = "Files"
                fileListData.MapColumnsToAttribute
                newXmlDataSet.Tables.Add(fileListData)
        End Select
        _UpdatedFrameListData = actualDataSet.Tables("Frames")
        Try
            newXmlDataSet.WriteXml(_xmlFileName)
        Catch ex As UnauthorizedAccessException
        End Try
    End Sub

    'shared functions

    ''' <summary>
    ''' Creates an framelist database with no records.
    ''' </summary>
    ''' <returns>The empty database.</returns>
    Public Shared Function EmptyFrameList() As DataTable
        Dim output = New DataTable("Frames")
        Dim columns = "Filename;Num;Filedate;Dossier;Drawing;Sheet;Descr1;Descr2;Descr3;Descr4;Client1;Client2;Client3;Client4;Project;Rev;FrameSize;Size;Scale;Design;Char;Date;Descr;Drawn;Check;LastRev_Char;LastRev_Date;LastRev_Descr;LastRev_Drawn;LastRev_Check"
        output.InsertColumns(columns.Cut)
        Return output
    End Function

    ''' <summary>
    ''' Creates an filelist database with no records.
    ''' </summary>
    ''' <returns>The empty database.</returns>
    Public Shared Function EmptyFileList() As DataTable
        Dim output = New DataTable("Files")
        Dim columns = "Filename;Filedate"
        output.InsertColumns(columns.Cut)
        Return output
    End Function

End Class
