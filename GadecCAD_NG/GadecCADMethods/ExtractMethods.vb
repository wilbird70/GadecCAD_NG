'Gadec Engineerings Software (c) 2022
Imports System.Data
Imports System.Windows.Forms
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.Geometry
Imports GadecCAD_NG.Extensions

''' <summary>
''' Provides methods to extraxt symbols and create legendblocks and csv-files.
''' </summary>
Public Class ExtractMethods

    ''' <summary>
    ''' Allows the user to select symbols (blockreferences) to extract them for creating csv-files.
    ''' <para>Note: The option to create a legendblock will be obsolete.</para>
    ''' </summary>
    ''' <param name="document">The present document.</param>
    Public Shared Sub ExtractSelection(document As Document)
        Dim db = document.Database
        Dim referenceIds = SelectMethods.GetSelectionOfReferences(document)
        If referenceIds.Count = 0 Then Exit Sub

        document.DocumentEvents.ProcessSymbolChanges(referenceIds)
        Dim referenceData = ReferenceHelper.GetReferenceData(db, referenceIds)
        Dim symbolData = ExtractMZXSymbols(referenceData)
        If AbortByDuplicateAddresses(symbolData) Then Exit Sub

        Dim items = "ExtractOptions".Translate.Cut
        Dim dialog = New ListBoxDialog("Select".Translate, items)
        If Not dialog.DialogResult = DialogResult.OK Then Exit Sub

        Select Case dialog.GetSelectedIndex
            Case 0 : ExtractToMZXConsysCSV(document.GetPath, symbolData)
            Case 1 : ExtractToCheckWinguardCSV(document.GetPath, symbolData)
            Case 2
                'MessageBoxInfo("Deze functie zal op termijn verdwijnen.{L}Gebruik in plaats hiervan 'Symbol Counter' [SYMCOUNTER]".NotYetTranslated)
                'CreateToLegendBlock_Obsolete(document, symbolData)
        End Select
    End Sub

    ''' <summary>
    ''' Extracts symbols from multiple documents to create csv-files.
    ''' </summary>
    ''' <param name="files">List of fullnames of files.</param>
    Public Shared Sub ExtractDocuments(files As String())
        Dim documents = DocumentsHelper.GetDocumentNames()
        Dim documentsToClose = New List(Of Document)
        Dim allSymbolData = New Data.DataTable
        Dim messageResult = MessageBoxQuestion("ReadFiles?".Translate(files.Count), MessageBoxButtons.YesNo)
        If messageResult = DialogResult.No Then Exit Sub

        Dim firstRow As DataRow = Nothing
        Try
            Progressbar = New ProgressShow("Starting...".Translate, files.Count)
            Dim existingKeys = New List(Of String)
            For Each file In files
                If Progressbar?.CancelPressed Then Exit For

                Progressbar?.PerformStep("{0}{1}".Compose("Opening...".Translate, FileSystemHelper.LimitDisplayLengthFileName(file, 60)))
                Dim doc = DocumentsHelper.Open(file)
                If IsNothing(doc) Then Continue For

                If IsNothing(firstRow) Then
                    Dim frameData = doc.FrameData
                    Select Case True
                        Case IsNothing(frameData)
                        Case frameData.Rows.Count = 0
                        Case Else : firstRow = frameData.Rows(0)
                    End Select
                End If
                Progressbar?.SetText("{0}{1}".Compose("Reading...".Translate, FileSystemHelper.LimitDisplayLengthFileName(file, 60)))

                Dim referenceIds = SelectHelper.GetAllReferencesInModelspace(doc)
                If referenceIds.Count > 0 Then
                    doc.DocumentEvents.ProcessSymbolChanges(referenceIds)
                    Dim referenceData = ReferenceHelper.GetReferenceData(doc.Database, referenceIds)
                    Dim symbolData = ExtractMZXSymbols(referenceData, existingKeys)
                    allSymbolData.Merge(symbolData)
                    existingKeys = allSymbolData.GetStringsFromColumn("WNGKEY").ToList
                End If

                If doc.WasClosed Then documentsToClose.Add(doc)
                If documentsToClose.Count < 10 Then Continue For

                DocumentsHelper.Close(documentsToClose.ToArray)
                documentsToClose.Clear()
            Next
        Catch ex As System.Exception
            ex.AddData($"{files.Count} files")
            ex.Rethrow
        Finally
            DocumentsHelper.Close(documentsToClose.ToArray)
            Progressbar?.Dispose()
            Progressbar = Nothing
        End Try
        If AbortByDuplicateAddresses(allSymbolData) Then Exit Sub

        Dim items = "ExtractOptions".Translate.Cut
        Dim dialog = New ListBoxDialog("Select".Translate, items)
        If Not dialog.DialogResult = DialogResult.OK Then Exit Sub

        Dim folder = IO.Path.GetDirectoryName(files(0))
        Select Case dialog.GetSelectedIndex
            Case 0 : ExtractToMZXConsysCSV(folder, allSymbolData)
            Case 1 : ExtractToCheckWinguardCSV(folder, allSymbolData)
            Case 2
                'MessageBoxInfo("Deze functie zal op termijn verdwijnen.{L}Gebruik in plaats hiervan 'Symbol Counter' [SYMCOUNTER].{2L}Een aparte tekening als legenda gaat verdwijnen.".NotYetTranslated)
                'CreateToLegendDrawing_Obsolete(firstRow, allSymbolData)
        End Select
    End Sub

    ''' <summary>
    ''' Allows the user to select symbols (blockreferences) to get a counting list and the ability to create a legendblock.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    Public Shared Sub SymbolCounter(document As Document)
        Dim db = document.Database
        Dim referenceIds = SelectMethods.GetSelectionOfReferences(document)
        If referenceIds.Count > 0 Then
            Dim symbolCounter = New Dictionary(Of String, Integer)
            Dim referenceData = ReferenceHelper.GetReferenceData(db, referenceIds)
            For Each row In referenceData.Select
                Dim blockName = row.GetString("BlockName")
                Select Case symbolCounter.ContainsKey(blockName)
                    Case True : symbolCounter(blockName) += 1
                    Case Else : symbolCounter.Add(blockName, 1)
                End Select
            Next
            Dim symbolDescriptions = DesignHelper.GetSymbolDescriptions()
            Dim clipbrdList = New List(Of String) 'lijst met gevonden omschrijvingen
            Dim unknownList = New List(Of String) 'lijst van onbekend symbolen
            For Each key In symbolCounter.Keys.ToSortedList
                If Not symbolDescriptions.ContainsKey(key) Then unknownList.Add("{0}{T}{1}{T}...{C}".Compose(symbolCounter(key), key)) : Continue For

                Dim description = symbolDescriptions(key)
                clipbrdList.Add("{0}{T}{1}{T}{2}{C}".Compose(symbolCounter(key), key, description))
            Next
            clipbrdList.AddRange(unknownList)
            If clipbrdList.Count > 0 Then
                Clipboard.SetText(String.Join(vbLf, clipbrdList))
                Dim iniString = "Met aantallen=DesignCenter legenda met aantallen;Zonder aantallen=DesignCenter legenda zonder aantallen;Incl. onbekend=Legenda met aantallen van (ook) onbekende symbolen".NotYetTranslated
                Dim buttons = iniString.Cut.ToIniDictionary
                Dim dialog = New MessageBoxDialog("Maken van legenda".NotYetTranslated, clipbrdList.ToArray, buttons)
                If dialog.DialogResult = DialogResult.OK Then
                    Select Case dialog.ButtonNumber
                        Case 0 : InsertLegendBlock(document, referenceData, True)
                        Case 1 : InsertLegendBlock(document, referenceData, False)
                        Case 2 : InsertLegendBlock(document, referenceData, True, True)
                    End Select
                End If
            End If
            Autodesk.AutoCAD.Internal.Utils.PostCommandPrompt()
        End If
    End Sub

    'Private subs

    ''' <summary>
    ''' Creates a mzx-consys csv-file of the specified (extracted) symbol database.
    ''' </summary>
    ''' <param name="initialFolder">Path to the folder where the saveas dialogbox will start.</param>
    ''' <param name="symbolData">The symbol database.</param>
    Private Shared Sub ExtractToMZXConsysCSV(initialFolder As String, symbolData As Data.DataTable)
        Dim consysData = DataSetHelper.LoadFromXml("{Support}\SetExtractInfo.xml".Compose).GetTable("Consys", "Name")
        If IsNothing(consysData) Then Exit Sub

        Dim fileName = FileSystemHelper.FileSaveAs("{0}\MZX-Consys.csv".Compose(initialFolder))
        If fileName = "" Then Exit Sub

        Dim symbolDescriptions = DesignHelper.GetSymbolDescriptions()
        Dim panels = New Dictionary(Of String, Boolean)

        Dim csvData = TextFileHelper.Read("{Resources}\MZX Consys Part1.csv".Compose).ToList
        csvData.Add("#Produced: {0}".Compose(Format(Now, "dd-MM-yyyy HH:mm:ss")))
        csvData.AddRange(TextFileHelper.Read("{Resources}\MZX Consys Part2.csv".Compose))

        Dim elementData = New List(Of String)
        For Each key In symbolData.GetUniqueStringsFromColumn("WNGKEY").ToSortedList
            Dim row = symbolData.Rows.Find(key)
            If row.HasValue("CTYPE") Then
                Dim consysType = row.GetString("CTYPE")
                Dim panel = row.GetString("PNR").Replace("P", "").ToInteger.ToString
                panels.TryAdd(panel, False)

                Dim consysRow = consysData.Rows.Find(consysType)
                If NotNothing(consysRow) Then
                    If consysRow.HasValue("PNL") Then
                        Dim panelType = consysRow.GetString("PNL")
                        csvData.Add("{0},{1},{Q}{2}{Q},{3}".Compose(panel, panelType, row.GetString("TYPE"), "CPU801"))
                        panels(panel) = True
                    End If
                    If consysRow.HasValue("CP") Then
                        Dim loopNumber = row.GetString("LS").Replace("-", "")
                        Dim address = row.GetString("ADR").ToInteger
                        Dim zone = row.GetString("GRP").ToInteger
                        Dim locationText = row.GetString("LOC").LeftString(19)
                        If locationText = "" Or locationText = " " Then
                            Select Case symbolDescriptions.ContainsKey(row.GetString("TYPE"))
                                Case True : locationText = symbolDescriptions(row.GetString("TYPE")).LeftString(19)
                                Case Else : locationText = "Unknown".Translate
                            End Select
                        End If
                        Dim properties = consysRow.GetString("CP")
                        If loopNumber = "" Then Continue For
                        If address = 0 Then Continue For
                        Dim elementRow = "{0},{1},{2},{3},{4},{5},{6},{Q}{7}{Q},{8}"
                        elementData.Add(elementRow.Compose(panel, loopNumber, address, "", zone, "", consysType, locationText, properties))
                    End If
                End If
            End If
        Next
        csvData.AddRange(TextFileHelper.Read("{Resources}\MZX Consys Part3.csv".Compose))
        csvData.AddRange(elementData)
        csvData.AddRange(TextFileHelper.Read("{Resources}\MZX Consys Part4.csv".Compose))
        TextFileHelper.Write(fileName, csvData.ToArray)

        Dim message = ""
        For Each key In panels.Keys.ToSortedList
            Select Case True
                Case panels(key)
                Case key = "0"
                Case Else : message &= "P{0}{L}".Compose("00{0}".Compose(key).RightString(2))
            End Select
        Next
        If Not message = "" Then message = "NoPanelinfo".Translate(message)

        Dim messageResult = MessageBoxQuestion("{0}{1}".Compose(message, "FileSaved".Translate), MessageBoxButtons.YesNo)
        If messageResult = DialogResult.Yes Then Shell("notepad {0}".Compose(fileName), AppWinStyle.NormalFocus)
    End Sub

    ''' <summary>
    ''' Creates a winguard check csv-file of the specified (extracted) symbol database.
    ''' </summary>
    ''' <param name="initialFolder">Path to the folder where the saveas dialogbox will start.</param>
    ''' <param name="symbolData">The symbol database.</param>
    Private Shared Sub ExtractToCheckWinguardCSV(initialFolder As String, symbolData As Data.DataTable)
        Dim fileName = FileSystemHelper.FileSaveAs("{0}\CheckWinguard.csv".Compose(initialFolder))
        If fileName = "" Then Exit Sub

        Dim elementData = New List(Of String)
        For Each key In symbolData.GetUniqueStringsFromColumn("WNGKEY").ToSortedList
            Dim row = symbolData.Rows.Find(key)
            If key.StartsWith("NonWinguard") Or row.GetString("WNGD") = "" Then Continue For

            elementData.Add("{0},{1}".Compose(key, row.GetString("BlockName")))
        Next
        TextFileHelper.Write(fileName, elementData.ToArray)
        Dim messageResult = MessageBoxQuestion("FileSaved".Translate, MessageBoxButtons.YesNo)
        If messageResult = DialogResult.Yes Then Shell("notepad {0}".Compose(fileName), AppWinStyle.NormalFocus)
    End Sub

    ''' <summary>
    ''' Allows the user to insert a legendblock in the present drawing.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="symbolData">The symbol database.</param>
    ''' <param name="withCounting">Whether symbol counting is desired in the legendblock.</param>
    ''' <param name="useAllSymbol">Whether all symbols (not just from DesignCenter) are desired in the legendblock.</param>
    Private Shared Sub InsertLegendBlock(document As Document, symbolData As Data.DataTable, withCounting As Boolean, Optional useAllSymbol As Boolean = False)
        Try
            Progressbar = New ProgressShow("Creating legend".Translate, 5)
            Dim ed = document.Editor
            Dim db = document.Database
            Dim definitionId As ObjectId
            Select Case useAllSymbol
                Case True : definitionId = CreateAllSymbolsLegendBlock(document, symbolData, withCounting)
                Case Else : definitionId = CreateDesignCenterLegendBlock(document, symbolData, withCounting)
            End Select
            Progressbar?.Dispose()
            If definitionId.IsNull Then Exit Sub

            Dim scale = If(db.ModelSpaceIsCurrent, CDbl(SysVarHandler.GetVar("DIMSCALE")), 1.0)
            Using unitless = New UnitlessInsertion(db)
                ed.UpdateScreen()
                DocumentEvents.CommandCanceled = False
                ed.Command("INSERT", "SymbolLegend", "SC", scale, "R", 0)
                If Not DocumentEvents.CommandCanceled Then ed.Command("EXPLODE", "L")
                DefinitionHelper.PurgeBlock(document, definitionId)
            End Using
        Catch ex As System.Exception
            ex.Rethrow
        Finally
            Progressbar?.Dispose()
            Progressbar = Nothing
        End Try
    End Sub

    'Private functions

    ''' <summary>
    ''' Shows a messagebox if there are duplicate addresses and asks if the user wants to abort.
    ''' </summary>
    ''' <param name="symbolData">The symbol database.</param>
    ''' <returns>True for abort.</returns>
    Private Shared Function AbortByDuplicateAddresses(symbolData As Data.DataTable) As Boolean
        Dim doubleData = symbolData.Select("DOUBLE=True")
        If doubleData.Count = 0 Then Return False

        Dim dialogMessage = {"Double Addr".Translate}.ToList
        Dim clipboardMessage = {"ReportDoubles".Translate}.ToList
        Dim symbolDescriptions = DesignHelper.GetSymbolDescriptions()
        Dim doubleAddresses = doubleData.CopyToDataTable.GetUniqueStringsFromColumn("WNGD").ToSortedList
        For Each address In doubleAddresses
            Dim rows = symbolData.Select("WNGD='{0}'".Compose(address))
            dialogMessage.Add("{0}x{T}{1}".Compose(rows.Count, address))
            For Each row In rows
                Dim blockName = row.GetString("BlockName")
                Dim point3d = row.GetPoint3d("Position")
                Dim position = "(x,y) {0} , {1}".Compose(point3d.X.ToString.Replace(",", "."), point3d.Y.ToString.Replace(",", "."))
                Dim layoutName = row.GetString("LayoutName")
                Dim fileName = row.GetString("FileName")
                Select Case symbolDescriptions.ContainsKey(blockName)
                    Case True : clipboardMessage.Add("{0}{T}{1}{T}{2}{T}{3}{T}{4}{T}{5}".Compose(address, blockName, symbolDescriptions(blockName), position, layoutName, fileName))
                    Case Else : clipboardMessage.Add("{0}{T}{1}{T}{2}{T}{3}{T}{4}{T}{5}".Compose(address, blockName, "...", position, layoutName, fileName))
                End Select
            Next
        Next
        Clipboard.SetText(String.Join("{CL}".Compose, clipboardMessage))
        Select Case MessageBoxQuestion("ResumeWithDoubles?".Translate(String.Join(vbLf, dialogMessage)), MessageBoxButtons.YesNo)
            Case DialogResult.No : Return True
            Case Else : Return False
        End Select
    End Function

    ''' <summary>
    ''' Extracts MZX symbols from the references database. 
    ''' </summary>
    ''' <param name="referenceData">The references database.</param>
    ''' <param name="existingKeys">List of existing keys when reading multiple documents.</param>
    ''' <returns>A MZX extracted references database.</returns>
    Private Shared Function ExtractMZXSymbols(referenceData As Data.DataTable, Optional existingKeys As List(Of String) = Nothing) As Data.DataTable
        If IsNothing(existingKeys) Then existingKeys = New List(Of String)
        referenceData.Columns.Add("WNGKEY", GetType(String))
        referenceData.Columns.Add("DOUBLE", GetType(Boolean))
        Dim output = referenceData.Clone
        output.AssignPrimaryKey("WNGKEY")
        Dim wng As String
        For Each row In referenceData.Select
            If Not row.HasAttribute("LVRC") Or Not row.HasAttribute("TYPE") Then Continue For

            Select Case row.HasValue("WNGD")
                Case True : wng = row.GetString("WNGD")
                Case Else : wng = "NonWinguard"
            End Select
            Dim key = wng
            Do While existingKeys.Contains(key)
                Select Case key.EndsWith("$")
                    Case True : key = key.AutoNumber
                    Case Else : key &= "_1$"
                End Select
            Loop
            existingKeys.Add(key)
            row("WNGKEY") = key
            Select Case True
                Case wng = "NonWinguard"
                Case key = wng
                Case row.GetString("CTYPE") = ""
                Case Else : row("DOUBLE") = True
            End Select
            output.ImportRow(row)
        Next
        Return output
    End Function

    ''' <summary>
    ''' Creates a legendblock from the reference database with only DesignCenter symbols.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="referenceData">The reference database.</param>
    ''' <param name="withCounting">Whether symbol counting is desired in the legendblock.</param>
    ''' <returns>Objectid of the definitions blocktablerecord.</returns>
    Private Shared Function CreateDesignCenterLegendBlock(document As Document, referenceData As Data.DataTable, withCounting As Boolean) As ObjectId
        Using importer = New DefinitionsImporter("{Resources}\TableBlocks.dwg".Compose)
            Dim output = importer.ImportDefinition(document, "SymbolLegend")
            If output = ObjectId.Null Then Return ObjectId.Null

            Using document.LockDocument
                Using tr = document.Database.TransactionManager.StartTransaction
                    Dim definition = tr.GetBlockTableRecord(output, OpenMode.ForWrite)
                    Dim tableIds = definition.ToArray.Where(Function(objectId) objectId.ObjectClass.DxfName.ToLower = "acad_table")
                    If tableIds.Count = 0 Then Return ObjectId.Null

                    Dim designCenterData = DesignHelper.GetAllDesignCenterData
                    Dim selectionData = designCenterData.Clone
                    Dim symbolData = referenceData.Select.Where(Function(row) Not row.GetValue("SlaveBlock")).CopyToDataTable

                    Dim usedBlocks = symbolData.GetUniqueStringsFromColumn("BlockName").ToList
                    Dim usedTypes = symbolData.GetUniqueStringsFromColumn("TYPE")
                    For Each usedType In usedTypes
                        Dim visibility = usedType.Cut("_").Last
                        If usedBlocks.Contains(visibility) Then Continue For

                        Select Case visibility
                            Case "BP", "OV", "K" : usedBlocks.Add(visibility)
                        End Select
                    Next

                    For Each usedBlock In usedBlocks
                        Dim selection = designCenterData.Select("BlockName = '_{0}'".Compose(usedBlock))
                        If selection.Count = 0 OrElse Not selection.First.HasValue("Legend") Then Continue For

                        Dim count = symbolData.Select("BlockName = '{0}'".Compose(usedBlock))
                        Select Case count.Count > 0
                            Case True : selection.First.SetString("Count", count.Count)
                            Case Else : selection.First.SetString("Count", "")
                        End Select
                        selectionData.ImportRow(selection.First)
                    Next

                    selectionData.AssignDefaultViewSort("Index")
                    Dim sortedSelectionData = selectionData.DefaultView.ToTable()
                    Dim table = tr.GetTable(tableIds(0), OpenMode.ForWrite)
                    Progressbar.SetMaximum(sortedSelectionData.Rows.Count)
                    Dim cellFormat = If(Translator.Selected = "EN", "{1}", "{0} / \fArial|b0|i1;{1}")
                    table.Cells(0, 0).TextString = cellFormat.Compose("LegendHeader_Legend".Translate, "Legend")
                    table.Cells(1, 0).TextString = cellFormat.Compose("LegendHeader_Symbol".Translate, "Symbol")
                    table.Cells(1, 1).TextString = cellFormat.Compose("LegendHeader_Type".Translate, "Type")
                    table.Cells(1, 2).TextString = cellFormat.Compose("LegendHeader_Number".Translate, "Number")
                    table.Cells(1, 3).TextString = cellFormat.Compose("LegendHeader_Description".Translate, "Description")
                    table.Cells(1, 4).TextString = cellFormat.Compose("LegendHeader_Typicals".Translate, "Typicals")

                    Dim symbolImport As DefinitionsImporter = Nothing
                    Try
                        Dim rowNumber = 1
                        For Each selectionRow In sortedSelectionData.Rows.ToArray
                            If Progressbar?.CancelPressed Then Exit For

                            rowNumber += 1
                            Dim symbolName = selectionRow.GetString("BlockName")
                            Progressbar?.PerformStep("Processing".Translate(symbolName))
                            table.InsertRows(rowNumber + 1, 6.75, 1)
                            table.Cells(rowNumber, 1).TextString = symbolName.EraseStart(1)
                            table.Cells(rowNumber, 2).TextString = selectionRow.GetString("Count")
                            Dim rotation = If(selectionRow.HasValue("Legend"), selectionRow.GetDouble("Legend") + 0.005, 0)
                            cellFormat = If(Translator.Selected = "EN", "{1}{L}", "{0}{L}\fArial|b0|i1;{1}")
                            table.Cells(rowNumber, 3).TextString = cellFormat.Compose(selectionRow.GetString(Translator.Selected), selectionRow.GetString("EN"))
                            Dim modl = selectionRow.GetString("Module")
                            Dim page = selectionRow.GetString("Page")

                            Dim sourceFile = designCenterData.GetTable("Pages").Rows.Find({modl, page}).GetString("SourceFile")
                            If IsNothing(symbolImport) OrElse Not symbolImport.FileName = sourceFile Then
                                symbolImport?.Dispose()
                                symbolImport = New DefinitionsImporter("{Resources}\{0}".Compose(sourceFile))
                            End If
                            Dim definitionId = symbolImport.ImportDefinition(document, symbolName)
                            If definitionId = ObjectId.Null Then Continue For

                            table.Cells(rowNumber, 0).BlockTableRecordId = definitionId
                            table.Cells(rowNumber, 0).Contents(0).Rotation = rotation * Math.PI / 180
                            table.Cells(rowNumber, 0).Contents(0).IsAutoScale = False
                            table.Cells(rowNumber, 0).Contents(0).Scale = 1
                        Next
                    Catch ex As System.Exception
                        ex.Rethrow
                    Finally
                        symbolImport?.Dispose()
                    End Try

                    table.DeleteColumns(5, 1)
                    table.DeleteColumns(4, 1)
                    If Not withCounting Then table.DeleteColumns(2, 1)
                    table.RecomputeTableBlock(True)
                    table.Position = New Point3d(0, 0, 0)
                    tr.Commit()
                End Using
            End Using
            Return output
        End Using
    End Function

    ''' <summary>
    ''' Creates a legendblock from the reference database.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="referenceData">The reference database.</param>
    ''' <param name="withCounting">Whether symbol counting is desired in the legendblock.</param>
    ''' <returns>Objectid of the blocktablerecord of the legendblock definition.</returns>
    Private Shared Function CreateAllSymbolsLegendBlock(document As Document, referenceData As Data.DataTable, withCounting As Boolean) As ObjectId
        Using importer = New DefinitionsImporter("{Resources}\TableBlocks.dwg".Compose)
            Dim output = importer.ImportDefinition(document, "SymbolLegend")
            If output = ObjectId.Null Then Return ObjectId.Null

            Using document.LockDocument
                Using tr = document.Database.TransactionManager.StartTransaction
                    Dim definition = tr.GetBlockTableRecord(output, OpenMode.ForWrite)
                    Dim tableIds = definition.ToArray.Where(Function(objectId) objectId.ObjectClass.DxfName.ToLower = "acad_table")
                    If tableIds.Count = 0 Then Return ObjectId.Null

                    Dim countField = New Data.DataColumn("Count", GetType(String))
                    referenceData.Columns.Add(countField)

                    Dim selectionData = referenceData.Clone
                    Dim usedBlocks = referenceData.GetUniqueStringsFromColumn("BlockName").ToList

                    For Each usedBlock In usedBlocks
                        Dim selection = referenceData.Select("BlockName = '{0}'".Compose(usedBlock))
                        Select Case selection.Count > 0
                            Case True : selection.First.SetString("Count", selection.Count)
                            Case Else : selection.First.SetString("Count", "")
                        End Select
                        selectionData.ImportRow(selection.First)
                    Next

                    selectionData.AssignDefaultViewSort("BlockName")
                    Dim sortedSelectionData = selectionData.DefaultView.ToTable()
                    Dim table = tr.GetTable(tableIds(0), OpenMode.ForWrite)
                    Progressbar.SetMaximum(sortedSelectionData.Rows.Count)
                    Dim cellFormat = If(Translator.Selected = "EN", "{1}", "{0} / \fArial|b0|i1;{1}")
                    table.Cells(0, 0).TextString = cellFormat.Compose("LegendHeader_Legend".Translate, "Legend")
                    table.Cells(1, 0).TextString = cellFormat.Compose("LegendHeader_Symbol".Translate, "Symbol")
                    table.Cells(1, 1).TextString = cellFormat.Compose("LegendHeader_Type".Translate, "Type")
                    table.Cells(1, 2).TextString = cellFormat.Compose("LegendHeader_Number".Translate, "Number")
                    table.Cells(1, 3).TextString = cellFormat.Compose("LegendHeader_Description".Translate, "Description")
                    table.Cells(1, 4).TextString = cellFormat.Compose("LegendHeader_Typicals".Translate, "Typicals")

                    Dim designCenterData = DesignHelper.GetAllDesignCenterData
                    Dim rowNumber = 1
                    For Each selectionRow In sortedSelectionData.Rows.ToArray
                        If Progressbar?.CancelPressed Then Exit For

                        rowNumber += 1
                        Dim symbolName = selectionRow.GetString("BlockName")
                        Dim designCenterRow = designCenterData.Select("BlockName = '_{0}'".Compose(symbolName)).FirstOrDefault
                        Progressbar?.PerformStep("Processing".Translate(symbolName))
                        table.InsertRows(rowNumber + 1, 6.75, 1)
                        table.Cells(rowNumber, 1).TextString = symbolName
                        table.Cells(rowNumber, 2).TextString = selectionRow.GetString("Count")
                        cellFormat = If(Translator.Selected = "EN", "{1}{L}", "{0}{L}\fArial|b0|i1;{1}")
                        table.Cells(rowNumber, 3).TextString = cellFormat.Compose(designCenterRow?.GetString(Translator.Selected), designCenterRow?.GetString("EN"))
                        Dim modl = selectionRow.GetString("Module")
                        Dim page = selectionRow.GetString("Page")

                        Dim definitionId = selectionRow.GetObjectId("DefinitionID")
                        Dim reference = New BlockReference(Point3d.Origin, definitionId)
                        Dim extents = If(reference.Bounds.HasValue, reference.GeometricExtents, reference.GeometryExtentsBestFit)
                        Dim width = extents.MaxPoint.X - extents.MinPoint.X
                        Dim height = extents.MaxPoint.Y - extents.MinPoint.Y
                        If width > 20 Or height > 20 Then Continue For

                        table.Cells(rowNumber, 0).BlockTableRecordId = definitionId
                        table.Cells(rowNumber, 0).Contents(0).Rotation = 0
                        table.Cells(rowNumber, 0).Contents(0).IsAutoScale = False
                        table.Cells(rowNumber, 0).Contents(0).Scale = 1
                    Next

                    table.DeleteColumns(5, 1)
                    table.DeleteColumns(4, 1)
                    If Not withCounting Then table.DeleteColumns(2, 1)
                    table.RecomputeTableBlock(True)
                    table.Position = New Point3d(0, 0, 0)
                    tr.Commit()
                End Using
            End Using
            Return output
        End Using
    End Function

    'obsolete private subs //////////////////////////////////////////////////////////////////////////////////////////

    '''' <summary>
    '''' Creates a legendblock from the reference database and allows the user to place it on the drawing.
    '''' </summary>
    '''' <param name="document">The present document.</param>
    '''' <param name="referenceData">The reference database.</param>
    'Private Shared Sub CreateToLegendBlock_Obsolete(document As Document, referenceData As Data.DataTable)
    '    Dim blockName = "MZX_Legend"
    '    Dim definitionId = GetLegendBlockAndRemoveUnusedSymbols_Obsolete(document, referenceData)
    '    If definitionId = ObjectId.Null Then Exit Sub

    '    Dim scale = If(document.Database.ModelSpaceIsCurrent, CDbl(SysVarHandler.GetVar("DIMSCALE")), 1.0)
    '    Dim command = "(command {Q}INSERT{Q} {Q}{0}{Q} {Q}SC{Q} {Q}{1}{Q} {Q}R{Q} 0 pause {Q}EXPLODE{Q} {Q}L{Q} {Q}-PURGE{Q} {Q}B{Q} {Q}{0}{Q} {Q}N{Q}) "
    '    document.SendString(command.Compose(blockName, scale))
    'End Sub

    '''' <summary>
    '''' Creates a legendblock from the reference database and places it in a new drawing.
    '''' </summary>
    '''' <param name="frameRow">The frame record.</param>
    '''' <param name="referenceData">The reference database.</param>
    'Private Shared Sub CreateToLegendDrawing_Obsolete(frameRow As DataRow, referenceData As Data.DataTable)
    '    DocumentEvents.DocumentEventsEnabled = True
    '    Dim doc = DocumentsHelper.CreateNew()
    '    Dim db = doc.Database
    '    Dim sheet = 0
    '    Dim definitionId = GetLegendBlockAndRemoveUnusedSymbols_Obsolete(doc, referenceData, sheet)
    '    If definitionId = ObjectId.Null Then Exit Sub

    '    Using doc.LockDocument
    '        Dim referenceId = ReferenceHelper.InsertReference(db, db.CurrentSpaceId, db.Clayer, definitionId, New Point3d(0, 0, 0))
    '        ReferenceHelper.ExplodeToOwnerSpace(db, referenceId)
    '        DefinitionHelper.PurgeBlock(db, definitionId)
    '    End Using
    '    Dim description = "LEGEND".Translate
    '    Dim drawingDate = Format(Now, "dd-MM-yyyy")
    '    Dim generated = "GENERATED".Translate
    '    Dim auto = "AUTO".Translate
    '    Dim overrideString = "Drawing=D-0001-L;Sheet=;Size=A4;Descr2={0};Descr3=;Rev=0;Scale=1:1;Date={1};Descr={2};Drawn={3};Check=;Sheets={4}"
    '    Dim overrideData = overrideString.Compose(description, drawingDate, generated, auto, sheet).Cut.ToIniDictionary
    '    Dim frameListRow = ConvertToFrameListRow_Obsolete(frameRow, overrideData)
    '    FrameInsertHelper.InsertFrames(doc, frameListRow)
    '    doc.ZoomExtents
    'End Sub

    '''' <summary>
    '''' Gets a full legendblock from the resource (TableBlocks.dwg) and removes all lines not mentioned in the reference database.
    '''' </summary>
    '''' <param name="document">The present document.</param>
    '''' <param name="referenceData">The reference database.</param>
    '''' <param name="sheets"></param>
    '''' <returns>Objectid of the blocktablerecord of the legendblock definition.</returns>
    'Private Shared Function GetLegendBlockAndRemoveUnusedSymbols_Obsolete(document As Document, referenceData As Data.DataTable, Optional ByRef sheets As Integer = -1) As ObjectId
    '    Dim dictionary = New Dictionary(Of String, Integer)
    '    Dim legendData = DataSetHelper.LoadFromXml("{Support}\SetExtractInfo.xml".Compose).GetTable("Legend", "Name")
    '    If IsNothing(legendData) Then Return ObjectId.Null

    '    For Each key In referenceData.GetUniqueStringsFromColumn("WNGKEY").ToSortedList
    '        Dim symbolRow = referenceData.Rows.Find(key)
    '        Dim type = symbolRow.GetString("TYPE")
    '        Dim legendRow = legendData.Rows.Find(type)
    '        Select Case True
    '            Case symbolRow.GetValue("SlaveBlock") : Continue For
    '            Case IsNothing(legendRow) : Continue For
    '        End Select
    '        Dim elementContent = legendRow.GetString("Content").Cut
    '        If elementContent(0) = "idem" Then elementContent(0) = type
    '        Select Case dictionary.ContainsKey(elementContent(0))
    '            Case True : dictionary(elementContent(0)) += 1
    '            Case Else : dictionary.Add(elementContent(0), 1)
    '        End Select
    '        For i = 1 To elementContent.Count - 1
    '            dictionary.TryAdd(elementContent(i), 0)
    '        Next
    '    Next

    '    Using importer = New DefinitionsImporter("{Resources}\TableBlocks.dwg".Compose)
    '        Dim output = importer.ImportDefinition(document, "MZX_Legend")
    '        If output = ObjectId.Null Then Return ObjectId.Null

    '        Using document.LockDocument
    '            Using tr = document.Database.TransactionManager.StartTransaction

    '                Dim definition = tr.GetBlockTableRecord(output, OpenMode.ForWrite)
    '                Dim tableIds = From objectId In definition.ToArray Where objectId.ObjectClass.DxfName.ToLower = "acad_table" Select objectId
    '                If tableIds.Count = 0 Then Return ObjectId.Null

    '                Dim table = tr.GetTable(tableIds(0), OpenMode.ForWrite)
    '                For i = table.Rows.Count - 1 To 2 Step -1
    '                    Dim celType = table.Cells(i, 1).TextString
    '                    Select Case True
    '                        Case Not dictionary.ContainsKey(celType) : table.DeleteRows(i, 1)
    '                        Case dictionary(celType) > 0 : table.Cells(i, 2).TextString = dictionary(celType)
    '                    End Select
    '                Next
    '                Select Case sheets = -1
    '                    Case True
    '                        table.DeleteColumns(5, 1)
    '                        table.DeleteColumns(4, 1)
    '                        table.DeleteColumns(2, 1)
    '                        table.RecomputeTableBlock(True)
    '                        table.Position = New Point3d(0, 0, 0)
    '                    Case Else
    '                        table.RecomputeTableBlock(True)
    '                        Dim tableExtents = table.GeometricExtents
    '                        sheets = Int((tableExtents.MaxPoint.X - tableExtents.MinPoint.X) / 260) + 1
    '                End Select
    '                tr.Commit()
    '            End Using
    '        End Using
    '        Return output
    '    End Using
    'End Function

    '''' <summary>
    '''' Converts the frame record to a framelist record, removes revision data, and overwrites some data with the specified data.
    '''' </summary>
    '''' <param name="frameRow">The frame record.</param>
    '''' <param name="overrideData">The data to override.</param>
    '''' <returns>A framelist record.</returns>
    'Private Shared Function ConvertToFrameListRow_Obsolete(frameRow As DataRow, overrideData As Dictionary(Of String, String)) As DataRow
    '    Dim dataBuilder = New DataBuilder("Framelist")
    '    For Each column In frameRow.GetAttributeColumns
    '        Select Case column.StartsWith("Rev#")
    '            Case True : dataBuilder.AppendValue(column, "")
    '            Case Else : dataBuilder.AppendValue(column, frameRow.GetString(column))
    '        End Select
    '    Next
    '    For Each pair In overrideData
    '        dataBuilder.AppendValue(pair.Key, pair.Value)
    '    Next
    '    dataBuilder.AddNewlyCreatedRow()
    '    Return dataBuilder.GetDataTable.Rows(0)
    'End Function

End Class
