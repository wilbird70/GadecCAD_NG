'Gadec Engineerings Software (c) 2022
Imports System.Data
Imports System.Windows.Forms
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports GadecCAD_NG.Extensions

''' <summary>
''' Provides methods for editing the data in frameheaders.
''' </summary>
Public Class FrameHeaderHelper
    ''' <summary>
    ''' The text applied when the value of an attribute from multiple drawings is not the same.
    ''' </summary>
    Private Shared ReadOnly _varies As String = "*VARIES*"

    'subs

    ''' <summary>
    ''' Shows a dialogbox to change the texts in the headers of multiple frames.
    ''' <para>A dictionary is used to pass the selection of frames in the following way:</para>
    ''' <para>- Key: The fullname of a document containing selected frames.</para>
    ''' <para>- Value: The num-codes of the selected frames, separated by semicolons.</para>
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="frameSelection">The dictionary containing the selection of frames.</param>
    ''' <param name="frameListData">The drawinglist database of the present project (folder).</param>
    Public Shared Sub EditHeader(document As Document, frameSelection As Dictionary(Of String, String), frameListData As Data.DataTable)
        frameListData.AssignPrimaryKey("Filename;Num") 'primarykey toewijzen
        Dim documents = DocumentsHelper.GetOpenDocuments()
        Dim adaptableRow = MergingSelectedFrameRecords(frameListData, frameSelection)
        Dim dialog = New HeaderDialog(adaptableRow, document.IsNamedDrawing, _varies)
        If Not dialog.DialogResult = DialogResult.OK Then Exit Sub

        Dim xmlListDataSet = DataSetHelper.LoadFromXml("{0}\Drawinglist.xml".Compose(IO.Path.GetDirectoryName(document.Name)))
        Dim xmlListData = If(xmlListDataSet.Tables.Contains("Frames"), xmlListDataSet.GetTable("Frames", "Filename;Num"), Nothing)
        Try
            If frameSelection.Count > 5 Then Progressbar = New ProgressShow("ProcessingDocuments".Translate, frameSelection.Count)
            Dim filesFailedToSave = New List(Of String)
            Dim files = frameSelection.Keys.ToList
            For Each file In files
                If Progressbar?.CancelPressed Then Exit For

                Dim textToChange = New Dictionary(Of ObjectId, String)
                Dim fileName = IO.Path.GetFileName(file)
                Progressbar?.PerformStep("Processing".Translate(fileName))
                Dim lock As DocumentLock = Nothing
                Dim db As Database = Nothing
                Try
                    Dim bitmap As Drawing.Bitmap = Nothing
                    Select Case documents.ContainsKey(file)
                        Case True
                            lock = documents(file).LockDocument()
                            db = documents(file).Database
                        Case Else
                            db = New Database(False, True)
                            db.ReadDwgFile(file, FileOpenMode.OpenForReadAndWriteNoShare, False, "")
                            db.CloseInput(True)
                            bitmap = db.ThumbnailBitmap
                    End Select
                    Dim frameIdCollections = XRecordObjectIdsHelper.Load(db, "{Company}".Compose, "FrameWorkIDs")
                    If frameIdCollections.Count = 0 Then Continue For

                    Dim frameData = FrameHelper.BuildFrameData(db, frameIdCollections)
                    frameData.AssignPrimaryKey("Num")
                    For Each number In frameSelection(file).Cut
                        Dim frameRow = frameData.Rows.Find(number)
                        If IsNothing(frameRow) Then Continue For

                        frameRow.SetString("Size", "")
                        frameRow.SetString("Scale", "")
                        Dim xmlListRow = xmlListData?.Rows.Find({fileName, number})
                        For Each column In frameRow.GetAttributeColumns
                            If Not adaptableRow.HasValue(column) Then Continue For

                            Select Case True
                                Case adaptableRow.GetString(column) = _varies
                                Case adaptableRow.GetString(column) = frameRow.GetString(column)
                                Case documents.ContainsKey(file)
                                    textToChange.TryAdd(frameRow.GetAttributeId(column), adaptableRow.GetString(column))
                                Case Else
                                    textToChange.TryAdd(frameRow.GetAttributeId(column), adaptableRow.GetString(column))
                                    If IsNothing(xmlListRow) Then Continue For

                                    xmlListRow.SetString(column, adaptableRow.GetString(column)) 'nieuwe data voor xml
                            End Select
                        Next
                    Next
                    If textToChange.Count = 0 Then Continue For

                    TextHelper.ChangeTextStrings(db, textToChange)
                    Select Case True
                        Case documents.ContainsKey(file)
                            documents(file).FrameData(True)
                            If Not file = document.Name Then documents(file).EditorNeedsRegen(True)
                        Case Else
                            Try
                                db.ThumbnailBitmap = bitmap
                                db.SaveAs(file, DwgVersion.Current)
                                Dim timeStamp = IO.File.GetLastWriteTime(file).ToTimeStamp
                                Dim rows = xmlListData?.Select("Filename='{0}'".Compose(fileName.Replace("'", "''")))
                                If NotNothing(rows) Then rows.ToList.ForEach(Sub(row) row("Filedate") = timeStamp)
                            Catch ex As Autodesk.AutoCAD.Runtime.Exception
                                filesFailedToSave.Add(IO.Path.GetFileName(file))
                            Catch ex As System.Exception
                                ex.Rethrow
                            End Try
                    End Select
                Catch ex As System.Exception
                    ex.Rethrow
                Finally
                    lock?.Dispose()
                    db?.Dispose()
                End Try
            Next
            If document.IsNamedDrawing Then xmlListData?.DataSet.WriteXml("{0}\Drawinglist.xml".Compose(IO.Path.GetDirectoryName(document.Name)))
            If filesFailedToSave.Count > 0 Then MsgBox("NotSavedFiles".Translate(String.Join(vbLf, filesFailedToSave)), MsgBoxStyle.Exclamation)
        Catch ex As System.Exception
            ex.AddData($"HeaderData: {String.Join(", ", adaptableRow.ItemArray)}")
            ex.Rethrow
        Finally
            Progressbar?.Dispose()
            Progressbar = Nothing
        End Try
    End Sub

    ''' <summary>
    ''' Shows a dialogbox to add a revision to the headers of multiple frames.
    ''' <para>A dictionary is used to pass the selection of frames in the following way:</para>
    ''' <para>- Key: The fullname of a document containing selected frames.</para>
    ''' <para>- Value: The num-codes of the selected frames, separated by semicolons.</para>
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="frameSelection">The dictionary containing the selection of frames.</param>
    Public Shared Sub AddRevision(document As Document, frameSelection As Dictionary(Of String, String))
        Dim documents = DocumentsHelper.GetOpenDocuments()
        Dim currentFileName = document.Name
        Dim dialog = New RevisionDialog(Registerizer.UserSetting("RevisionDrawnIni"), Registerizer.UserSetting("RevisionCheckIni"))
        If Not dialog.DialogResult = DialogResult.OK Then Exit Sub

        Dim revisionData = dialog.GetRevisionData.ToIniDictionary
        Registerizer.UserSetting("RevisionDrawnIni", revisionData("Drawn"))
        Registerizer.UserSetting("RevisionCheckIni", revisionData("Check"))
        If Not IsDate(revisionData("Date")) Then Exit Sub

        Dim xmlListDataSet = DataSetHelper.LoadFromXml("{0}\Drawinglist.xml".Compose(IO.Path.GetDirectoryName(document.Name)))
        Dim xmlListData = If(xmlListDataSet.Tables.Contains("Frames"), xmlListDataSet.GetTable("Frames", "Filename;Num"), Nothing)
        Try
            If frameSelection.Count > 5 Then Progressbar = New ProgressShow("ProcessingDocuments".Translate, frameSelection.Count)
            Dim filesFailedToSave = New List(Of String)
            Dim files = frameSelection.Keys.ToList
            For Each file In files
                If Progressbar?.CancelPressed Then Exit For

                Dim sortToChange = New Dictionary(Of ObjectId, String)
                Dim textToChange = New Dictionary(Of ObjectId, String)
                Dim fileName = IO.Path.GetFileName(file)
                Progressbar?.PerformStep("Processing".Translate(fileName))
                Dim lock As DocumentLock = Nothing
                Dim db As Database = Nothing
                Try
                    Dim bitmap As Drawing.Bitmap = Nothing
                    Select Case documents.ContainsKey(file)
                        Case True
                            lock = documents(file).LockDocument()
                            db = documents(file).Database
                        Case Else
                            db = New Database(False, True)
                            db.ReadDwgFile(file, FileOpenMode.OpenForReadAndWriteNoShare, False, "")
                            db.CloseInput(True)
                            bitmap = db.ThumbnailBitmap
                    End Select
                    Dim frameIdCollections = XRecordObjectIdsHelper.Load(db, "{Company}".Compose, "FrameWorkIDs")
                    If frameIdCollections.Count = 0 Then Continue For

                    Dim frameData = FrameHelper.BuildFrameData(db, frameIdCollections)
                    frameData.AssignPrimaryKey("Num")
                    If documents.ContainsKey(file) Then lock = documents(file).LockDocument()
                    For Each number In frameSelection(file).Cut
                        Dim frameRow = frameData.Rows.Find(number)
                        If IsNothing(frameRow) Then Continue For

                        Dim sortedRevisions = SortRevisions(frameRow)
                        Dim addedRevision = AddRevision(frameRow, revisionData)
                        sortToChange = sortToChange.Union(sortedRevisions).ToDictionary(Function(d) d.Key, Function(d) d.Value)
                        textToChange = textToChange.Union(addedRevision).ToDictionary(Function(d) d.Key, Function(d) d.Value)
                        If documents.ContainsKey(file) Then Continue For

                        Dim frameListRow = xmlListData?.Rows.Find({fileName, number})
                        If IsNothing(frameListRow) Then Continue For

                        frameListRow.SetString("LastRev_Date", revisionData("Date"))
                        frameListRow.SetString("LastRev_Char", revisionData("Char"))
                        frameListRow.SetString("LastRev_Drawn", revisionData("Drawn"))
                        frameListRow.SetString("LastRev_Descr", revisionData("Descr"))
                        frameListRow.SetString("LastRev_Check", revisionData("Check"))
                    Next
                    If textToChange.Count = 0 And sortToChange.Count = 0 Then Continue For

                    TextHelper.ChangeTextStrings(db, sortToChange)
                    TextHelper.ChangeTextStrings(db, textToChange)
                    Select Case True
                        Case documents.ContainsKey(file)
                            documents(file).FrameData(True)
                            If Not file = document.Name Then documents(file).EditorNeedsRegen(True)
                        Case Else
                            Try
                                db.ThumbnailBitmap = bitmap
                                db.SaveAs(file, DwgVersion.Current)
                                Dim timeStamp = IO.File.GetLastWriteTime(file).ToTimeStamp
                                Dim rows = xmlListData?.Select("Filename='{0}'".Compose(fileName.Replace("'", "''")))
                                If NotNothing(rows) Then rows.ToList.ForEach(Sub(row) row("Filedate") = timeStamp)
                            Catch ex As Autodesk.AutoCAD.Runtime.Exception
                                filesFailedToSave.Add(IO.Path.GetFileName(file))
                            Catch ex As System.Exception
                                ex.Rethrow
                            End Try
                    End Select
                Catch ex As System.Exception
                    ex.Rethrow
                Finally
                    lock?.Dispose()
                    db?.Dispose()
                End Try
            Next
            If document.IsNamedDrawing Then xmlListData?.DataSet.WriteXml("{0}\Drawinglist.xml".Compose(IO.Path.GetDirectoryName(document.Name)))
            If filesFailedToSave.Count > 0 Then MsgBox("NotSavedFiles".Translate(String.Join(vbLf, filesFailedToSave)), MsgBoxStyle.Exclamation)
        Catch ex As System.Exception
            ex.AddData($"RevisionData: {String.Join(", ", revisionData)}")
            ex.Rethrow
        Finally
            Progressbar?.Dispose()
            Progressbar = Nothing
        End Try
    End Sub

    '////////// frameless //////////

    ''' <summary>
    ''' Shows a dialogbox to change the texts in the header (with the absence of a frame) located in the current layout.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    Public Shared Sub EditFramelessHeader(document As Document)
        Dim headerRow = FrameHelper.BuildFramelessRow(document)
        If IsNothing(headerRow) Then Exit Sub

        Dim adaptableRow = FrameHelper.EmptyFramelist.NewRow
        adaptableRow("Filename") = "Dummy"
        adaptableRow("Num") = "1"
        For Each column In headerRow.GetAttributeColumns
            Dim textString As String
            Select Case column
                Case "Size"
                    Select Case headerRow.GetString("FrameSize") = "$*"
                        Case True : textString = "$*{0}".Compose(headerRow.GetString(column))
                        Case Else : textString = headerRow.GetString("FrameSize")
                    End Select
                Case "Scale"
                    Select Case headerRow.GetString("FrameSize") = "$*"
                        Case True : textString = "$*{0}".Compose(headerRow.GetString(column))
                        Case Else : textString = FrameHelper.GetScaleFactor(document.Database, headerRow.GetValue("ObjectID"))
                    End Select
                Case Else
                    textString = headerRow.GetString(column)
            End Select
            Select Case True
                Case Not adaptableRow.Table.Columns.Contains(column)
                Case Not adaptableRow.HasValue(column) : adaptableRow(column) = textString
                Case Not adaptableRow.GetString(column) = textString : adaptableRow(column) = _varies
            End Select
        Next
        adaptableRow.Table.Rows.Add(adaptableRow)
        Dim dialog = New HeaderDialog(adaptableRow, document.IsNamedDrawing, _varies)
        If Not dialog.DialogResult = DialogResult.OK Then Exit Sub

        Dim textToChange = New Dictionary(Of ObjectId, String)
        Dim newHdrStr = dialog.GetHeader
        For Each column In headerRow.GetAttributeColumns
            If newHdrStr.HasValue(column) Then
                Select Case column
                    Case "Size"
                        Select Case True
                            Case Not headerRow.GetString("FrameSize") = "$*" : newHdrStr(column) = headerRow.GetString("FrameSize")
                            Case Not headerRow.GetString(column) = newHdrStr.GetString(column) : textToChange.TryAdd(headerRow.GetAttributeId(column), newHdrStr.GetString(column))
                        End Select
                    Case "Scale"
                        If Not headerRow.GetString("FrameSize") = "$*" Then
                            newHdrStr(column) = FrameHelper.GetScaleFactor(document.Database, headerRow.GetValue("ObjectID"))
                        End If
                End Select
                Select Case True
                    Case newHdrStr.GetString(column) = _varies
                    Case newHdrStr.GetString(column) = headerRow.GetString(column)
                    Case Else : textToChange.TryAdd(headerRow.GetAttributeId(column), newHdrStr.GetString(column))
                End Select
            End If
        Next
        TextHelper.ChangeTextStrings(document, textToChange)
    End Sub

    ''' <summary>
    ''' Shows a dialogbox to add a revision to the header (with the absence of a frame) located in the current layout.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    Public Shared Sub AddFramelessRevision(document As Document)
        Dim frameRow = FrameHelper.BuildFramelessRow(document)
        Dim dialog = New RevisionDialog(Registerizer.UserSetting("RevisionDrawnIni"), Registerizer.UserSetting("RevisionCheckIni"))
        If Not dialog.DialogResult = DialogResult.OK Then Exit Sub

        Dim revisionData = dialog.GetRevisionData.ToIniDictionary
        Registerizer.UserSetting("RevisionDrawnIni", revisionData("Drawn"))
        Registerizer.UserSetting("RevisionCheckIni", revisionData("Check"))
        If Not IsDate(revisionData("Date")) Then Exit Sub

        Dim sortedRevisions = SortRevisions(frameRow)
        Dim addedRevision = AddRevision(frameRow, revisionData)
        Dim sortToChange = New Dictionary(Of ObjectId, String)
        Dim textToChange = New Dictionary(Of ObjectId, String)
        sortToChange = sortToChange.Union(sortedRevisions).ToDictionary(Function(d) d.Key, Function(d) d.Value)
        textToChange = textToChange.Union(addedRevision).ToDictionary(Function(d) d.Key, Function(d) d.Value)
        TextHelper.ChangeTextStrings(document, sortToChange)
        TextHelper.ChangeTextStrings(document, textToChange)
    End Sub

    'private functions

    ''' <summary>
    ''' Merges the texts from the headers of multiple frames. Like AutoCAD's designation, it's labeled '*VARIES*' when items are not equal. 
    ''' <para>A dictionary is used to pass the selection of frames in the following way:</para>
    ''' <para>- Key: The fullname of a document containing selected frames.</para>
    ''' <para>- Value: The num-codes of the selected frames, separated by semicolons.</para>
    ''' </summary>
    ''' <param name="frameListData">The drawinglist database.</param>
    ''' <param name="frameSelection">The dictionary containing the selection of frames.</param>
    ''' <returns>The merged framerecord.</returns>
    Private Shared Function MergingSelectedFrameRecords(frameListData As Data.DataTable, frameSelection As Dictionary(Of String, String)) As DataRow
        Dim output = FrameHelper.EmptyFramelist.NewRow
        output("Filename") = "Dummy"
        output("Num") = "1"
        Dim files = frameSelection.Keys.ToList
        For Each file In files
            Dim fileName = IO.Path.GetFileName(file)
            For Each number In frameSelection(file).Cut
                Dim framweRow = frameListData.Rows.Find({fileName, number})
                If IsNothing(framweRow) Then Continue For

                For Each column In framweRow.GetColumnNames
                    If Not framweRow.HasValue(column) Then Continue For

                    Select Case True
                        Case Not output.Table.Columns.Contains(column)
                        Case Not output.HasValue(column) : output(column) = framweRow.GetString(column)
                        Case Not output.GetString(column) = framweRow.GetString(column) : output(column) = _varies
                    End Select
                Next
            Next
        Next
        output.Table.Rows.Add(output)
        Return output
    End Function

    ''' <summary>
    ''' Sorts the revisions by date, so that they are in readable order.
    ''' </summary>
    ''' <param name="frameRow">The present framerecord.</param>
    ''' <returns>A dictionary containing textchanges for the attributes.</returns>
    Private Shared Function SortRevisions(ByRef frameRow As DataRow) As Dictionary(Of ObjectId, String)
        Dim revisions = New List(Of Dictionary(Of String, String))
        Dim countNoDate = 0
        Dim countIsDate = 0
        'collect and sort the revision data.
        Dim position = 1
        Do While frameRow.HasAttribute("Rev#{0}_Date".Compose(position))
            Dim revision = TransposeRevision(frameRow, position)
            Select Case True
                Case revision("Date") = "" : revisions.Add(revision)
                Case IsDate(revision("Date"))
                    For j = countNoDate To countNoDate + countIsDate
                        Select Case True
                            Case j = countNoDate + countIsDate : revisions.Insert(j, revision)
                            Case CDate(revision("Date")) < CDate(revisions(j)("Date")) : revisions.Insert(j, revision) : Exit For
                        End Select
                    Next
                    countIsDate += 1
                Case Else : revisions.Insert(countNoDate, revision) : countNoDate += 1
            End Select
            position += 1
        Loop
        'collect attribute changes.
        Dim output = New Dictionary(Of ObjectId, String)
        For i = 0 To revisions.Count - 1
            For Each pair In revisions(i)
                Dim columnName = "Rev#{0}_{1}".Compose(i + 1, pair.Key)
                If Not frameRow(columnName) = pair.Value Then output.TryAdd(frameRow.GetAttributeId(columnName), pair.Value)
                frameRow(columnName) = pair.Value
            Next
        Next
        Return output
    End Function

    ''' <summary>
    ''' Reads the revision on the specified position from the framerecord into a dictionary.
    ''' </summary>
    ''' <param name="frameRow">The present framerecord.</param>
    ''' <param name="position">The position of revisiondata.</param>
    ''' <returns>The dictionary with a revision.</returns>
    Private Shared Function TransposeRevision(frameRow As DataRow, position As Integer) As Dictionary(Of String, String)
        Dim output = New Dictionary(Of String, String)
        For Each key In "Char;Drawn;Descr;Check".Cut
            Dim tag = "Rev#{0}_{1}".Compose(position, key)
            If frameRow.HasAttribute(tag) Then output.Add(key, frameRow.GetString(tag))
        Next
        output.Add("Date", DateStringHelper.Convert(frameRow.GetString("Rev#{0}_Date".Compose(position))))
        Return output
    End Function

    ''' <summary>
    ''' Adds a new revision to the framerecord. If the revisiondate is the same as the last revisiondate, revisiondata is overridden.
    ''' </summary>
    ''' <param name="frameRow">The present framerecord.</param>
    ''' <param name="revisionData">The new revisiondata.</param>
    ''' <returns>A dictionary containing textchanges for the attributes.</returns>
    Private Shared Function AddRevision(ByRef frameRow As DataRow, ByRef revisionData As Dictionary(Of String, String)) As Dictionary(Of ObjectId, String)
        Dim revisionPosition = 0
        Dim revisionsCount = 1
        Dim isNew = False
        revisionData("Char") = frameRow.GetString("LastRev_Char")
        'iterate through revisions to determine current position and number of positions.
        Do While frameRow.HasAttribute("Rev#{0}_Date".Compose(revisionsCount))
            Select Case frameRow.GetString("Rev#{0}_Date".Compose(revisionsCount))
                Case "", " ", "  ", "   "
                Case Else : revisionPosition = revisionsCount
            End Select
            revisionsCount += 1
        Loop
        revisionsCount -= 1
        'determine if it is a new revision date.
        Select Case True
            Case frameRow.GetString("LastRev_Char") = "0", frameRow.GetString("LastRev_Char") = "" : isNew = True
            Case Not IsDate(frameRow.GetString("LastRev_Date")) : isNew = True
            Case CDate(frameRow.GetString("LastRev_Date")) < CDate(revisionData("Date")) : isNew = True
        End Select
        'determine new revision characters.
        Select Case True
            Case Not isNew 'no new characters needed.
            Case frameRow.GetString("LastRev_Char") = "0", frameRow.GetString("LastRev_Char") = "", frameRow.GetString("LastRev_Char") = " "
                revisionData("Char") = "A"
            Case frameRow.GetString("LastRev_Char") = "Z"
                revisionData("Char") = "AA"
            Case frameRow.GetString("LastRev_Char").Length = 1
                revisionData("Char") = Chr(frameRow.GetString("LastRev_Char").GetAscii(1) + 1)
            Case Else 'double characters.
                Dim characters = frameRow.GetString("LastRev_Char").ToArray
                Select Case characters(1) = "Z"
                    Case True : revisionData("Char") = Chr(characters(0).GetAscii + 1) & "A"
                    Case Else : revisionData("Char") = characters(0) & Chr(characters(1).GetAscii + 1)
                End Select
        End Select
        revisionData("KOPREV") = revisionData("Char") & " (" & revisionData("Drawn") & ")"
        Dim output = New Dictionary(Of ObjectId, String)
        Dim position As Integer
        Select Case True
            Case Not isNew
                'no new revision date, current revision will be overwritten.
                position = revisionPosition
            Case revisionPosition < revisionsCount
                'revision will be inserted on next open position.
                position = revisionPosition + 1
            Case Else
                'positions are full, move up revisions, revision on last position.
                For i = 2 To revisionsCount
                    Dim moveUp = CopyRevision(frameRow, i, i - 1)
                    output.AddRange(moveUp.ToArray)
                Next
                position = revisionsCount
        End Select
        Dim insert = InsertRevision(frameRow, position, revisionData)
        output.AddRange(insert.ToArray)
        Return output
    End Function

    ''' <summary>
    ''' Copies a revision from one position to another.
    ''' </summary>
    ''' <param name="frameRow">The present framerecord.</param>
    ''' <param name="from">The position to copy from.</param>
    ''' <param name="to">The position to copy to.</param>
    ''' <returns>A dictionary containing textchanges for attributes.</returns>
    Private Shared Function CopyRevision(frameRow As DataRow, from As Integer, [to] As Integer) As Dictionary(Of ObjectId, String)
        Dim output = New Dictionary(Of ObjectId, String)
        For Each key In "Date;Char;Drawn;Descr;Check".Cut
            Dim tag1 = "Rev#{0}_{1}".Compose(from, key)
            Dim tag2 = "Rev#{0}_{1}".Compose([to], key)
            If frameRow.HasAttribute(tag2) Then output.Add(frameRow.GetAttributeId(tag2), frameRow.GetString(tag1))
        Next
        Return output
    End Function

    ''' <summary>
    ''' Inserts the revisiondata on the specified position.
    ''' </summary>
    ''' <param name="frameRow">The present framerecord.</param>
    ''' <param name="position">The position to use.</param>
    ''' <param name="revisionData">The new revisiondata.</param>
    ''' <returns>A dictionary containing textchanges for attributes.</returns>
    Private Shared Function InsertRevision(frameRow As DataRow, position As Integer, revisionData As Dictionary(Of String, String)) As Dictionary(Of ObjectId, String)
        Dim output = New Dictionary(Of ObjectId, String)
        For Each key In "Date;Char;Drawn;Descr;Check".Cut
            Dim tag = "Rev#{0}_{1}".Compose(position, key)
            If frameRow.HasAttribute(tag) Then output.Add(frameRow.GetAttributeId(tag), revisionData(key))
        Next
        If frameRow.HasAttribute("Rev") Then output.Add(frameRow.GetAttributeId("Rev"), revisionData("Char"))
        Return output
    End Function

End Class
