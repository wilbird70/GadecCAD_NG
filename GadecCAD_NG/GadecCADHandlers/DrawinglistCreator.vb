'Gadec Engineerings Software (c) 2022
Imports System.Data
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.Geometry
Imports GadecCAD_NG.Extensions

''' <summary>
''' <para><see cref="DrawinglistCreator"/> can create an AutoCAD drawing with a coversheet and drawinglist.</para>
''' </summary>
Public Class DrawinglistCreator
    Implements IDisposable
    Private _disposed As Boolean

    ''' <summary>
    ''' The selection of frames to process.
    ''' </summary>
    ''' <returns>The dictionary containing the selection of frames.</returns>
    Public ReadOnly Property Selection As Dictionary(Of String, String)
    ''' <summary>
    ''' The name of the project.
    ''' </summary>
    ''' <returns>The projectname.</returns>
    Public ReadOnly Property ProjectName As String

    ''' <summary>
    ''' The drawinglist headerdata.
    ''' </summary>
    Private ReadOnly _drawinglistHeader As DataRow
    ''' <summary>
    ''' The selected framelist database.
    ''' </summary>
    Private ReadOnly _selectedFrameList As Data.DataTable
    ''' <summary>
    ''' The created drawinglist drawing.
    ''' </summary>
    Private _database As Database = Nothing

    'class

    ''' <summary>
    ''' Initializes a new instance of <see cref="DrawinglistCreator"/> with the specified selection from the specified framelist database.
    ''' <para><see cref="DrawinglistCreator"/> can create an AutoCAD drawing with a coversheet and drawinglist.</para>
    ''' <para>A dictionary is used to pass the selection of frames in the following way:</para>
    ''' <para>- Key: The fullname of a document containing selected frames.</para>
    ''' <para>- Value: The num-codes of the selected frames, separated by semicolons.</para>
    ''' </summary>
    ''' <param name="frameSelection">The dictionary containing the selection of frames.</param>
    ''' <param name="frameListData">The framelist database of the present project (folder).</param>
    ''' <param name="prefixLength">The number of characters for the systemnumber.</param>
    Public Sub New(frameSelection As Dictionary(Of String, String), frameListData As Data.DataTable, prefixLength As Integer)
        Selection = RemoveDLSTfromSelection(frameSelection)
        If IsNothing(Selection) Then Exit Sub

        _selectedFrameList = GetSelectedFrameList(frameListData)
        If _selectedFrameList.Rows.Count = 0 Then Exit Sub

        _drawinglistHeader = GetDrawinglistHeader(_selectedFrameList, If(prefixLength < 1, 4, prefixLength))
    End Sub

    ''' <summary>
    ''' Unlocks (disposes) the drawing.
    ''' </summary>
    Protected Overridable Sub Dispose(disposing As Boolean)
        If _disposed Then Exit Sub

        If disposing Then _database?.Dispose()
        _disposed = True
    End Sub

    ''' <summary>
    ''' Implements the dispose method.
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
        Dispose(disposing:=True)
        GC.SuppressFinalize(Me)
    End Sub

    'subs

    ''' <summary>
    ''' Creates an AutoCAD drawing with a coversheet and drawinglist.
    ''' </summary>
    ''' <param name="addStatusStamp">If True, adds a stamp for statusindication and signing.</param>
    ''' <param name="attachments">A list of attachments that will be listed in the drawinglist.</param>
    ''' <param name="onlyAttachments">If true, no drawings will be listed in the drawinglist.</param>
    Public Sub Create(Optional addStatusStamp As Boolean = False, Optional attachments As String() = Nothing, Optional onlyAttachments As Boolean = False)
        Progressbar = New ProgressShow("Generate Coversheet and Drawinglist".Translate, 8, True)
        Dim newDatabase = New Database(True, True)
        Progressbar?.PerformStep()
        Using tr = newDatabase.TransactionManager.StartTransaction
            Using import = New DefinitionsImporter("{Resources}\TableBlocks.dwg".Compose)
                Dim coverDefinitionId = import.ImportDefinition(newDatabase, "Coversheet")
                Progressbar?.PerformStep()
                Dim listDefinitionId = import.ImportDefinition(newDatabase, "Drawinglist")
                If coverDefinitionId = ObjectId.Null Or listDefinitionId = ObjectId.Null Then
                    Progressbar?.Dispose()
                    Progressbar = Nothing
                    newDatabase.Dispose()
                    Exit Sub
                End If

                Progressbar?.PerformStep()
                Select Case addStatusStamp
                    Case True
                        _ProjectName = _drawinglistHeader.GetString("ProjectName")
                        _drawinglistHeader.SetString("Drawing", "DWGLIST")
                        'invoegen signeerstempel
                        Dim definitionId = import.ImportDefinition(newDatabase, "JCI_STAMP2021")
                        If Not definitionId.IsNull Then
                            Dim newStampId = ReferenceHelper.InsertReference(newDatabase, SymbolUtilityServices.GetBlockModelSpaceId(newDatabase), newDatabase.Clayer, definitionId, New Point3d(120, 77, 0))
                            Dim entities = ReferenceHelper.ExplodeToOwnerSpace(newDatabase, newStampId)
                            For Each entitiy In entities
                                If entitiy.GetDBObjectType = "BlockReference" Then
                                    Dim blkInfo = ReferenceHelper.GetReferenceData(newDatabase, entitiy.ObjectId)
                                    Dim textToChange = New Dictionary(Of ObjectId, String) From {{blkInfo.GetAttributeId("REVTEXTS"), _drawinglistHeader.GetString("Status")}}
                                    TextHelper.ChangeTextStrings(newDatabase, textToChange)
                                End If
                            Next
                        End If
                    Case Else
                        _ProjectName = "{0}_DLST".Compose(_drawinglistHeader.GetString("System"))
                        _drawinglistHeader.SetString("Drawing", _ProjectName)
                End Select
                Progressbar?.PerformStep()
                Dim coverDefinition = tr.GetBlockTableRecord(coverDefinitionId, OpenMode.ForWrite)
                Dim coverTableIds = From objectId In coverDefinition.ToArray Where objectId.ObjectClass.DxfName.ToLower = "acad_table" Select objectId
                If coverTableIds.Count > 0 Then
                    Dim table = tr.GetTable(coverTableIds(0), OpenMode.ForWrite)
                    table.Cells(2, 0).TextString = "Dossier".Translate
                    table.Cells(5, 0).TextString = "System".Translate
                    table.Cells(9, 0).TextString = "Enduser".Translate
                    WriteCoversheet(table, _drawinglistHeader)
                End If
                Progressbar?.PerformStep()
                Dim listDefinition = tr.GetBlockTableRecord(listDefinitionId, OpenMode.ForWrite)
                Dim listTableIds = From objectId In listDefinition.ToArray Where objectId.ObjectClass.DxfName.ToLower = "acad_table" Select objectId
                If listTableIds.Count > 0 Then
                    Dim table = tr.GetTable(listTableIds(0), OpenMode.ForWrite)
                    table.Cells(3, 1).TextString = "Date".Translate
                    table.Cells(4, 1).TextString = "Project".Translate
                    table.Cells(3, 3).TextString = "Size".Translate
                    table.Cells(4, 3).TextString = "Scale".Translate
                    table.Cells(3, 5).TextString = "Revision".Translate
                    table.Cells(4, 5).TextString = "RevDescr".Translate
                    If Not onlyAttachments Then
                        If _selectedFrameList.Rows.Count > 1 Then table.InsertRows(5, 4, (_selectedFrameList.Rows.Count - 1) * 3)
                        Dim sourceRange = CellRange.Create(table, 2, 0, 4, 6)
                        For j = 1 To _selectedFrameList.Rows.Count - 1
                            Dim rowNumber = (j * 3) + 2
                            Dim targetRange = CellRange.Create(table, rowNumber, 0, rowNumber + 2, 6)
                            table.CopyFrom(table, TableCopyOptions.FillTarget, sourceRange, targetRange)
                            WriteDrawingListRecord(table, _selectedFrameList.Rows(j), j)
                        Next
                        WriteDrawingListRecord(table, _selectedFrameList.Rows(0), 0)
                    End If
                    table.Cells(0, 0).TextString = "DRAWINGLIST".Translate
                    If NotNothing(attachments) Then
                        Dim rowNumber = If(onlyAttachments, 5, (_selectedFrameList.Rows.Count * 3) + 2)
                        For Each attachment In attachments
                            table.InsertRowsAndInherit(rowNumber, 0, 1)
                            table.UnmergeCells(table.Rows(rowNumber))
                            table.MergeCells(CellRange.Create(table, rowNumber, 1, rowNumber, 6))
                            table.Rows(rowNumber).Height = 12
                            Dim name = IO.Path.GetFileNameWithoutExtension(attachment)
                            Dim pages = PdfSharpHelper.GetNumberOfPages(attachment)
                            table.Cells(rowNumber, 0).TextString = "Attachment".Translate
                            Select Case pages
                                Case 0 : table.Cells(rowNumber, 1).TextString = "No Page".Translate(name)
                                Case 1 : table.Cells(rowNumber, 1).TextString = "One page".Translate(name)
                                Case Else : table.Cells(rowNumber, 1).TextString = "Pages".Translate(name, pages)
                            End Select
                            table.Rows(rowNumber).TextHeight = 2.5
                            rowNumber += 1
                        Next
                        If onlyAttachments Then table.DeleteRows(2, 3)
                    End If
                End If
                Progressbar?.PerformStep()
                ReferenceHelper.InsertReference(newDatabase, SymbolUtilityServices.GetBlockModelSpaceId(newDatabase), newDatabase.Clayer, coverDefinitionId, New Point3d(0, 0, 0))
                Progressbar?.PerformStep()
                ReferenceHelper.InsertReference(newDatabase, SymbolUtilityServices.GetBlockModelSpaceId(newDatabase), newDatabase.Clayer, listDefinitionId, New Point3d(0, 0, 0))
                _drawinglistHeader("Sheets") = Int(_selectedFrameList.Rows.Count / 18) + 2
                Progressbar?.PerformStep()
                FrameInsertHelper.InsertFrames(newDatabase, _drawinglistHeader)
                Progressbar?.Dispose()
                Progressbar = Nothing
            End Using
            tr.Commit()
        End Using
        _database = newDatabase
    End Sub

    Public Sub SaveAs(fileName As String)
        _database.SaveAs(fileName, DwgVersion.Current)
    End Sub

    'functions

    Public Function CreateFailure() As Boolean
        Return IsNothing(_database)
    End Function

    Public Function NoSelection() As Boolean
        Return IsNothing(_Selection)
    End Function

    Public Function GetDatabase() As Database
        Return _database
    End Function

    ''' <summary>
    ''' Gets the drawinglist headerdata.
    ''' </summary>
    ''' <returns>A <see cref="DataRow"/></returns>
    Public Function GetDrawinglistHeader() As DataRow
        Return _drawinglistHeader
    End Function

    'private subs

    ''' <summary>
    ''' Writes data from the drawinglistheader to the coversheet.
    ''' </summary>
    ''' <param name="table">The drawingtable to write to.</param>
    ''' <param name="drawinglistHeader"></param>
    Private Sub WriteCoversheet(table As Table, drawinglistHeader As DataRow)
        For Each column In drawinglistHeader.GetColumnNames
            Dim value = drawinglistHeader.GetString(column)
            Select Case column
                Case "Dossier" : table.Cells(3, 0).TextString = value
                Case "System" : table.Cells(6, 0).TextString = value
                Case "Description1" : table.Cells(7, 0).TextString = value
                Case "Description2" : table.Cells(8, 0).TextString = value
                Case "Client1" : table.Cells(10, 0).TextString = value
                Case "Client2" : table.Cells(11, 0).TextString = value
                Case "Client3" : table.Cells(12, 0).TextString = value
                Case "Client4" : table.Cells(13, 0).TextString = value
            End Select
        Next
    End Sub

    ''' <summary>
    ''' Writes data from the framelist to a drawinglistrecord.
    ''' </summary>
    ''' <param name="table">The drawingtable to write to.</param>
    ''' <param name="frameListRow">A framelist record.</param>
    ''' <param name="recordNumber">The recordnumber.</param>
    Private Sub WriteDrawingListRecord(table As Table, frameListRow As DataRow, recordNumber As Integer)
        Dim rowNumber = (recordNumber * 3) + 2
        Dim descr = If(frameListRow.GetString("Descr") = "-", "", frameListRow.GetString("Descr"))
        Dim dated = If(frameListRow.GetString("Date") = "-", "", frameListRow.GetString("Date"))
        For Each column In frameListRow.GetColumnNames
            Dim value = If(frameListRow.GetString(column) = "-", "", frameListRow.GetString(column))
            Select Case column
                Case "Drawing" : SetCellText(table.Cells(rowNumber, 0), "{0}{1}".Compose(value, table.Cells(rowNumber, 0).TextString))
                Case "Sheet" : SetCellText(table.Cells(rowNumber, 0), "{0} {1}".Compose(table.Cells(rowNumber, 0).TextString, value))
                Case "Descr1" : SetCellText(table.Cells(rowNumber, 1), value)
                Case "Descr2" : SetCellText(table.Cells(rowNumber, 3), value)
                Case "Descr3" : SetCellText(table.Cells(rowNumber, 5), value)
                Case "Date" : SetCellText(table.Cells(rowNumber + 1, 2), value)
                Case "Size" : SetCellText(table.Cells(rowNumber + 1, 4), value)
                Case "Project" : SetCellText(table.Cells(rowNumber + 2, 2), value)
                Case "Scale" : SetCellText(table.Cells(rowNumber + 2, 4), value)
                Case "LastRev_Descr"
                    Select Case frameListRow.GetString("LastRev_Date") = ""
                        Case True : SetCellText(table.Cells(rowNumber + 2, 6), descr)
                        Case Else : SetCellText(table.Cells(rowNumber + 2, 6), value)
                    End Select
                Case "LastRev_Char" : SetCellText(table.Cells(rowNumber + 1, 6), "{0}{1}".Compose(value, table.Cells(rowNumber + 1, 6).TextString))
                Case "LastRev_Date"
                    If value = "" Then value = dated
                    Select Case value = ""
                        Case True : SetCellText(table.Cells(rowNumber + 1, 6), "")
                        Case Else : SetCellText(table.Cells(rowNumber + 1, 6), "{0} ({1})".Compose(table.Cells(rowNumber + 1, 6).TextString, value))
                    End Select
            End Select
        Next
    End Sub

    ''' <summary>
    '''Sets the text in the cell and shortens the text if it is too long.
    ''' </summary>
    ''' <param name="cell">The cell where the text should be placed.</param>
    ''' <param name="text">The text to be placed in the cell.</param>
    Private Sub SetCellText(cell As Cell, text As String)
        Dim extents = cell.GetExtents()
        Dim distance = extents(1).X - extents(0).X
        Do While Autodesk.AutoCAD.Internal.Utils.GetTextExtents(cell.TextStyleId, text, cell.TextHeight).X > distance
            text = text.EraseEnd(1)
        Loop
        cell.TextString = text
    End Sub

    'Private functions

    ''' <summary>
    ''' Removes the drawinglist drawings from the selection.
    ''' </summary>
    ''' <param name="frameSelection">The dictionary containing the selection of frames.</param>
    ''' <returns>The dictionary containing the selection of frames without drawinglist drawings.</returns>
    Private Function RemoveDLSTfromSelection(frameSelection As Dictionary(Of String, String)) As Dictionary(Of String, String)
        Dim exclusiveSelection = frameSelection.ToList
        exclusiveSelection.ForEach(Sub(pair) If pair.Key.EndsWith("_DLST.dwg") Then exclusiveSelection.Remove(pair))
        Dim output = exclusiveSelection.ToDictionary(Function(d) d.Key, Function(d) d.Value)
        Select Case output.Count > 0
            Case True : Return output
            Case Else : Return Nothing
        End Select
    End Function

    ''' <summary>
    ''' Gets the framelist database of the selected frames.
    ''' </summary>
    ''' <param name="frameListData">The framelist database of the present project (folder).</param>
    ''' <returns>The selected framelist database.</returns>
    Private Function GetSelectedFrameList(frameListData As Data.DataTable)
        Dim output = FrameHelper.EmptyFramelist
        frameListData.AssignPrimaryKey("Filename;Num")
        For Each file In _Selection.Keys
            Dim fileName = IO.Path.GetFileName(file)
            For Each number In _Selection(file).Cut
                Dim frameListRow = frameListData.Rows.Find({fileName, number})
                If NotNothing(frameListRow) Then output.ImportRow(frameListRow)
            Next
        Next
        Return output
    End Function

    ''' <summary>
    ''' Creates the drawinglist headerdata.
    ''' </summary>
    ''' <returns>A <see cref="DataRow"/></returns>
    Private Function GetDrawinglistHeader(frameListData As Data.DataTable, prefixLength As Integer) As DataRow
        Dim dataBuilder = New DataBuilder("Framelist")
        Dim columns = frameListData.GetColumnNames
        For Each row In frameListData.Rows.ToArray
            For Each column In columns
                If dataBuilder.GetString(column) = "" Then dataBuilder.AppendValue(column, row.GetString(column))
            Next
        Next
        dataBuilder.AppendValue("Description1", dataBuilder.GetString("Descr1"))
        dataBuilder.AppendValue("Description2", "")
        dataBuilder.AppendValue("Descr1", "")
        dataBuilder.AppendValue("Descr2", "[COVERSHEET;DRAWINGLIST]".Translate)
        dataBuilder.AppendValue("Descr3", "")
        dataBuilder.AppendValue("Descr4", "")
        dataBuilder.AppendValue("Rev", "0")
        dataBuilder.AppendValue("Size", "A4")
        dataBuilder.AppendValue("Scale", "1:1")
        dataBuilder.AppendValue("Date", Format(Now, "dd-MM-yyyy"))
        dataBuilder.AppendValue("Descr", "GENERATED".Translate)
        dataBuilder.AppendValue("Drawn", "AUTO".Translate)
        dataBuilder.AppendValue("Check", "")
        dataBuilder.AppendValue("Sheets", "")
        dataBuilder.AppendValue("Sheet", "")
        dataBuilder.AppendValue("System", dataBuilder.GetString("Drawing").LeftString(prefixLength))
        dataBuilder.AddNewlyCreatedRow()
        Return dataBuilder.GetDataTable.Rows(0)
    End Function

End Class
