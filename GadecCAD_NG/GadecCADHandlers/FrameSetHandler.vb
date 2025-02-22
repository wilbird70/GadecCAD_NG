'Gadec Engineerings Software (c) 2022
Imports System.Data
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports GadecCAD_NG.Extensions

Public Class FrameSetHandler
    Public ReadOnly Property UpdatedFrameListData As Data.DataTable
    Private ReadOnly _fileName As String
    Private ReadOnly _folder As String
    Private ReadOnly _justSaved As Boolean
    Private ReadOnly _documents As Dictionary(Of String, Document)
    Private ReadOnly _frameSet As FrameSetModel
    Private ReadOnly _readFilesForNewFrameSet As New List(Of String)
    Private ReadOnly _readFilesForXmlFrameSet As New List(Of String)
    Private ReadOnly _folderHasWritePermission As Boolean = True

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

    Private Function TimeStampHasChanged(dataRows As DataRow(), timeStamp As String) As Boolean
        For Each row In dataRows
            If Not row.GetString("Filedate") = timeStamp Then Return True
        Next
        Return False
    End Function

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

End Class
