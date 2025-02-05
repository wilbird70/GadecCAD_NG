'Gadec Engineerings Software (c) 2022
Imports System.Data
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports GadecCAD_NG.Extensions

''' <summary>
''' <para><see cref="FrameSetHandler"/> controles the frame database collections.</para>
''' <para>There are two collections:</para>
''' <para>- The first only with the data of the saved documents.</para>
''' <para>- The other includes the modifications in open documents.</para>
''' <para>Each collection has two databases:</para>
''' <para>- One for the data of all frames in the documents of the current project (folder).</para>
''' <para>- The other with the names of documents where no frames were found.</para>
''' </summary>
Public Class FrameSetHandler
    ''' <summary>
    ''' Gets the updated framelist database. This database belongs to a database collection that also contains the filelist database.
    ''' </summary>
    ''' <returns>The updated framelist database.</returns>
    Public ReadOnly Property UpdatedFrameListData As Data.DataTable

    ''' <summary>
    ''' The fullname of the current document.
    ''' </summary>
    Private ReadOnly _fileName As String
    ''' <summary>
    ''' The path of the current folder.
    ''' </summary>
    Private ReadOnly _folder As String
    ''' <summary>
    ''' Indicates whether the document has just been saved
    ''' </summary>
    Private ReadOnly _justSaved As Boolean
    ''' <summary>
    ''' Has all the open documents.
    ''' </summary>
    Private ReadOnly _documents As Dictionary(Of String, Document)
    ''' <summary>
    ''' A <see cref="FrameSetModel"/> which contains the frame database collections.
    ''' </summary>
    Private ReadOnly _frameSet As FrameSetModel
    ''' <summary>
    ''' List of fullnames of opened and/or modified documents.
    ''' </summary>
    Private ReadOnly _readFilesForNewFrameSet As New List(Of String)
    ''' <summary>
    ''' List of fullnames of closed (or just saved) modified documents.
    ''' </summary>
    Private ReadOnly _readFilesForXmlFrameSet As New List(Of String)
    ''' <summary>
    ''' Indicates whether the user has write permission in current folder.
    ''' </summary>
    Private ReadOnly _folderHasWritePermission As Boolean = True

    'class

    ''' <summary>
    ''' Initializes a new instance of <see cref="FrameSetHandler"/> for the specified document and updates the frame database collections for the current project (folder) and saves it as a Drawinglist.xml-file.
    ''' <para><see cref="FrameSetHandler"/> controles the frame database collections.</para>
    ''' <para>There are two collections:</para>
    ''' <para>- The first only with the data of the saved documents.</para>
    ''' <para>- The other includes the modifications in open documents.</para>
    ''' <para>Each collection has two databases:</para>
    ''' <para>- One for the data of all frames in the documents of the current project (folder).</para>
    ''' <para>- The other with the names of documents where no frames were found.</para>
    ''' </summary>
    ''' <param name="fileName">The fullname of the document.</param>
    ''' <param name="justSaved">Specify if the document is just saved.</param>
    Public Sub New(fileName As String, justSaved As Boolean)
        _fileName = fileName
        _folder = IO.Path.GetDirectoryName(fileName)
        _justSaved = justSaved
        _documents = DocumentsHelper.GetOpenDocuments()
        _folderHasWritePermission = FileSystemHelper.FolderHasWritePermission(_folder)

        Try
            Dim xmlFileName = "{0}\Drawinglist.xml".Compose(_folder)
            _frameSet = New FrameSetModel(xmlFileName)
            CompareLastWriteTimesAndCopyUnmodified()
            ReadDataFromOpenedAndModifiedDocuments()
            _frameSet.Save()
            UpdatedFrameListData = _frameSet.UpdatedFrameListData
        Catch ex As System.IO.IOException
            MessageBoxInfo("File system error".Translate)
            UpdatedFrameListData = FrameSetModel.EmptyFrameList
        End Try
    End Sub

    'private subs

    ''' <summary>
    ''' Compares lastwritetimes with the datetimes in the Drawinglist.xml-file and copy unmodified data.
    ''' </summary>
    Private Sub CompareLastWriteTimesAndCopyUnmodified()
        For Each file In IO.Directory.GetFiles(_folder, "*.dwg")
            Dim selectString = "Filename='{0}'".Compose(IO.Path.GetFileName(file).Replace("'", "''"))
            Dim currentFrameRows = _frameSet.FrameListData.Select(selectString)
            Dim currentFileRows = _frameSet.FileListData.Select(selectString)
            Select Case True
                Case file = _fileName
                    Select Case True
                        Case _justSaved : _readFilesForXmlFrameSet.Add(file)
                        Case currentFrameRows.Count > 0 : _frameSet.AddToSavedFrameList(currentFrameRows)
                        Case currentFileRows.Count > 0 : _frameSet.AddToSavedFileList(currentFileRows.First)
                    End Select
                    _readFilesForNewFrameSet.Add(file)
                Case _documents.ContainsKey(file)
                    Select Case True
                        Case currentFrameRows.Count > 0 : _frameSet.AddToSavedFrameList(currentFrameRows)
                        Case currentFileRows.Count > 0 : _frameSet.AddToSavedFileList(currentFileRows.First)
                    End Select
                    _readFilesForNewFrameSet.Add(file)
                Case currentFrameRows.Count > 0
                    Select Case TimeStampHasChanged(currentFrameRows, IO.File.GetLastWriteTime(file).ToTimeStamp)
                        Case True : _readFilesForNewFrameSet.Add(file) : _readFilesForXmlFrameSet.Add(file)
                        Case Else : _frameSet.AddToSavedFrameList(currentFrameRows) : _frameSet.AddToActualFrameList(currentFrameRows)
                    End Select
                Case currentFileRows.Count > 0
                    Select Case TimeStampHasChanged(currentFileRows, IO.File.GetLastWriteTime(file).ToTimeStamp)
                        Case True : _readFilesForNewFrameSet.Add(file) : _readFilesForXmlFrameSet.Add(file)
                        Case Else : _frameSet.AddToSavedFileList(currentFileRows.First) : _frameSet.AddToActualFileList(currentFileRows.First)
                    End Select
                Case Else : _readFilesForNewFrameSet.Add(file) : _readFilesForXmlFrameSet.Add(file)
            End Select
        Next
    End Sub

    ''' <summary>
    ''' Reads the data from the opened and/or modified documents.
    ''' </summary>
    Private Sub ReadDataFromOpenedAndModifiedDocuments()
        Progressbar?.Dispose()
        If _folderHasWritePermission And _readFilesForXmlFrameSet.Count > 4 Then
            Progressbar = New ProgressShow("ReadingDocuments".Translate, _readFilesForXmlFrameSet.Count)
        End If
        For Each file In _readFilesForNewFrameSet
            Dim db As Database = Nothing
            Try
                Select Case True
                    Case Progressbar?.CancelPressed 'drawing stays Nothing
                    Case _documents.ContainsKey(file)
                        db = _documents(file).Database
                    Case Not _folderHasWritePermission 'drawing stays Nothing
                    Case Else
                        Progressbar?.PerformStep("Reading".Translate(IO.Path.GetFileName(file)))
                        db = New Database(False, True)
                        db.ReadDwgFile(file, FileOpenMode.OpenForReadAndAllShare, True, "")
                End Select
                Dim frameIdCollections = XRecordObjectIdsHelper.Load(db, "{Company}".Compose, "FrameWorkIDs")
                Select Case frameIdCollections.Count = 0
                    Case True
                        Dim fileRow = _frameSet.FileListData.NewRow
                        fileRow("Filename") = IO.Path.GetFileName(file)
                        fileRow("Filedate") = IO.File.GetLastWriteTime(file).ToTimeStamp
                        _frameSet.AddToActualFileList(fileRow)
                        If _readFilesForXmlFrameSet.Contains(file) Then _frameSet.AddToSavedFileList(fileRow)
                    Case Else
                        Using tr = db.TransactionManager.StartTransaction
                            For Each pair In frameIdCollections
                                Dim frameRow = _frameSet.FrameListData.NewRow
                                frameRow("Filename") = IO.Path.GetFileName(file)
                                frameRow("Filedate") = IO.File.GetLastWriteTime(file).ToTimeStamp
                                frameRow("Num") = pair.Key
                                AppendHeaderData(tr, frameRow, pair.Value)
                                _frameSet.AddToActualFrameList({frameRow})
                                If _readFilesForXmlFrameSet.Contains(file) Then _frameSet.AddToSavedFrameList({frameRow})
                            Next
                            tr.Commit()
                        End Using
                End Select
            Catch ex As System.Exception
                Dim newRow = _frameSet.FileListData.NewRow
                newRow("Filename") = IO.Path.GetFileName(file)
                newRow("Filedate") = IO.File.GetLastWriteTime(file).ToTimeStamp
                _frameSet.AddToActualFileList(newRow)
                _frameSet.AddToSavedFileList(newRow)
            Finally
                db?.Dispose()
            End Try
        Next
        Progressbar?.Dispose()
        Progressbar = Nothing
    End Sub

    'private functions

    ''' <summary>
    ''' Appends the texts in the header to the record.
    ''' <para>Each frame is identified by an <see cref="ObjectIdCollection"/> containing the objectids of the blockreferences representing the frame and its headers.</para>
    ''' </summary>
    ''' <param name="transaction">The present transaction.</param>
    ''' <param name="frameRow">The frame record.</param>
    ''' <param name="frameIds">The <see cref="ObjectIdCollection"/> containing the objectids of the frame and its headers.</param>
    Private Sub AppendHeaderData(transaction As Transaction, frameRow As DataRow, frameIds As ObjectIdCollection)
        Dim revisions = New RevisionModel

        Dim hasFrame = False
        Dim dataSet = DataSetHelper.LoadFromXml("{Support}\SetFramesInfo.xml".Compose) 'done
        Dim frameInfoData = dataSet.GetTable("Frames", "Name")
        Dim headerInfoData = dataSet.GetTable("Headers", "Name")
        If IsNothing(frameInfoData) Or IsNothing(headerInfoData) Then Exit Sub

        For i = 0 To frameIds.Count - 1
            Dim frameOrHeader = transaction.GetBlockReference(frameIds(i))
            If IsNothing(frameOrHeader) Then Continue For

            Dim family As String
            Select Case i
                Case 0 'is frame
                    Dim frameInfoRow = frameInfoData.Rows.Find(frameOrHeader.Name.Cut("$").First)
                    If IsNothing(frameInfoRow) Then Exit Sub

                    hasFrame = True
                    frameRow("FrameSize") = frameInfoRow.GetString("Frame")
                    family = frameInfoRow.GetString("AttFam")
                Case Else
                    Dim headerInfoRow = headerInfoData.Rows.Find(frameOrHeader.Name)
                    If IsNothing(headerInfoRow) Then Continue For

                    family = headerInfoRow.GetString("AttFam")
            End Select
            Dim familyInfoData = dataSet.GetTable(family, "Name")
            If IsNothing(familyInfoData) Then Continue For

            Dim attributeTags = New List(Of String)
            Dim attributeIds = frameOrHeader.AttributeCollection
            For Each attributeId In attributeIds.ToArray
                Dim attribute = transaction.GetAttributeReference(attributeId)
                Dim attributeTag = attribute.Tag
                Do While attributeTags.Contains(attributeTag)
                    attributeTag = attributeTag.AutoNumber
                Loop
                attributeTags.Add(attributeTag)
                Dim familyInfoRow = familyInfoData.Rows.Find(attributeTag)
                If IsNothing(familyInfoRow) Then Continue For

                Dim infoTag = familyInfoRow.GetString("Info")
                Select Case infoTag.StartsWith("Rev#")
                    Case True : revisions.AddRevisionData(infoTag, familyInfoRow.GetString("Revision"), attribute.TextString)
                    Case Else : frameRow(infoTag) = attribute.TextString
                End Select
            Next
        Next
        revisions.CopyLastRevisionToDataRow(frameRow)

        If hasFrame Then
            frameRow("Scale") = FrameHelper.GetScaleFactor(transaction, frameIds(0))
            Dim frameSize = frameRow.GetValue("FrameSize")
            If NotNothing(frameSize) Then frameRow("Size") = frameSize
        End If
    End Sub

    ''' <summary>
    ''' Compares the timestamp in the records with the specified timestamp.
    ''' </summary>
    ''' <param name="dataRows">A frame- or filerecord.</param>
    ''' <param name="timeStamp">The timestamp.</param>
    ''' <returns>True if a timestamp is different.</returns>
    Private Function TimeStampHasChanged(dataRows As DataRow(), timeStamp As String) As Boolean
        For Each row In dataRows
            If Not row.GetString("Filedate") = timeStamp Then Return True
        Next
        Return False
    End Function

End Class
