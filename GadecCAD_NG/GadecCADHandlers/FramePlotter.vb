'Gadec Engineerings Software (c) 2022
Imports System.Data
'Imports System.Windows.Forms
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.EditorInput
Imports Autodesk.AutoCAD.Geometry
Imports Autodesk.AutoCAD.PlottingServices
Imports GadecCAD_NG.Extensions

''' <summary>
''' <para><see cref="FramePlotter"/> can plot frames to plotter, printer or pdf-file.</para>
''' </summary>
Public Class FramePlotter
    ''' <summary>
    ''' The database that contains the device- and colortable-information for the departments.
    ''' </summary>
    Private ReadOnly _departmentData As Data.DataTable
    ''' <summary>
    ''' The database that contains the plotoptions.
    ''' </summary>
    Private ReadOnly _optionData As Data.DataTable
    ''' <summary>
    ''' The plotoption (name) to use.
    ''' </summary>
    Private ReadOnly _plotOption As String
    ''' <summary>
    ''' The database that contains the device information.
    ''' </summary>
    Private ReadOnly _deviceData As Data.DataTable
    ''' <summary>
    ''' The dictionary containing the selection of frames.
    ''' </summary>
    Private ReadOnly _frameSelection As Dictionary(Of String, String)

    ''' <summary>
    ''' The flag that determines whether plot should be sent to a file (pdf plotting).
    ''' </summary>
    Private _plotToFile As Boolean

    'class

    ''' <summary>
    ''' Initializes a new instance of <see cref="FramePlotter"/> with the specified selection and plotoption.
    ''' <para>It also reads the SetPlotSettings.xml for the department-, option- and device-information.</para>
    ''' <para><see cref="FramePlotter"/> can plot frames to plotter, printer or pdf-file.</para>
    ''' <para>A dictionary is used to pass the selection of frames in the following way:</para>
    ''' <para>- Key: The fullname of a document containing selected frames.</para>
    ''' <para>- Value: The num-codes of the selected frames, separated by semicolons.</para>
    ''' </summary>
    ''' <param name="frameSelection">The dictionary containing the selection of frames.</param>
    ''' <param name="plotOption">The plotoption (name) to use.</param>
    Public Sub New(frameSelection As Dictionary(Of String, String), plotOption As String)
        Dim dataSet = DataSetHelper.LoadFromXml("{Support}\SetPlotSettings.xml".Compose)
        _departmentData = dataSet.GetTable("Departments", "Name")
        _optionData = dataSet.GetTable("Options", "Name;FrameSize")
        _deviceData = dataSet.GetTable("Devices", "Name;PaperSize")
        _frameSelection = frameSelection
        _plotOption = plotOption
    End Sub

    'frames

    ''' <summary>
    ''' Plots all frames in a drawing without the need to open the document in the drawing viewer.
    ''' </summary>
    ''' <param name="database">The drawing that contains the frames to plot.</param>
    ''' <param name="folder">The folder for temporary files.</param>
    ''' <returns>An array of fullnames of temporary files.</returns>
    Public Function PlotExternalDrawing(database As Database) As String()
        If Not PlotFactory.ProcessPlotState = ProcessPlotState.NotPlotting Then MessageBoxInfo("Another plot".Translate) : Return {}

        FileSystemHelper.CreateFolder("{TempFolder}".Compose)
        Dim output = New List(Of String)
        Dim errorMessage = New List(Of String)
        Dim frameIdCollections = XRecordObjectIdsHelper.Load(database, "{Company}".Compose, "FrameWorkIDs")
        Dim frameData = FrameHelper.BuildFrameData(database, frameIdCollections)
        frameData.AssignPrimaryKey("Sheet")
        Dim plotSession = frameData.GetStringsFromColumn("Sheet").ToSortedList
        Dim previousDwg = HostApplicationServices.WorkingDatabase
        Try
            HostApplicationServices.WorkingDatabase = database
            Dim sheet = 0
            Using plotEngine = PlotFactory.CreatePublishEngine()
                Using plotProgressDialog = New PlotProgressDialog(False, plotSession.Count, True)
                    For Each number In plotSession
                        Dim frameRow = frameData.Rows.Find(number)
                        If IsNothing(frameRow) Then errorMessage.Add("Sheet {0} not found".NotYetTranslated(number)) : Continue For

                        LayoutManager.Current.CurrentLayout = frameRow.GetString("LayoutName")
                        Dim plotSettings = WireUpPlotSettings(database, frameRow)
                        If IsNothing(plotSettings) Then Continue For

                        Dim plotInfo = WireUpPlotInfo(plotSettings, frameRow)
                        sheet += 1
                        If sheet = 1 Then
                            InitializePlotProgressDialog(plotProgressDialog, plotSession.Count)
                            plotEngine.BeginPlot(plotProgressDialog, Nothing)
                        End If

                        Dim fileName = "{TempFolder}\_{0}".Compose(Randomizer.GetString(12))
                        plotEngine.BeginDocument(plotInfo, "CoverAndIndex", Nothing, 1, _plotToFile, fileName)
                        output.Add("{0}.pdf".Compose(fileName))

                        Dim drawingSheetNumber = "{0}-{1}".Compose(frameRow.GetString("Drawing"), frameRow.GetString("Sheet"))
                        PerformPagePlotProgressDialog(plotProgressDialog, drawingSheetNumber, sheet, plotSession.Count)
                        ProcessingPage(plotEngine, plotInfo, plotProgressDialog, True, False)
                        plotEngine.EndDocument(Nothing)
                    Next
                    plotProgressDialog.OnEndPlot()
                    If sheet > 0 Then plotEngine.EndPlot(Nothing)
                End Using
            End Using
        Catch ex As System.Exception
            ex.AddData("Number of frames = {0}".NotYetTranslated(frameData.Rows.Count))
            ex.Rethrow
        Finally
            HostApplicationServices.WorkingDatabase = previousDwg
        End Try
        If errorMessage.Count > 0 Then MsgBox(String.Join(vbLf, errorMessage))
        Return output.ToArray
    End Function

    ''' <summary>
    ''' Plot the selected frames separately from multiple documents.
    ''' </summary>
    ''' <param name="copies">Number of copies to plot.</param>
    ''' <returns>An array of fullnames of temporary files.</returns>
    Public Function PlotMultiFrame(copies As Integer) As String()
        If Not PlotFactory.ProcessPlotState = ProcessPlotState.NotPlotting Then MessageBoxInfo("Another plot".Translate) : Return {}

        FileSystemHelper.CreateFolder("{TempFolder}".Compose)
        Dim output = New List(Of String)
        Dim errorMessage = New List(Of String)
        Dim documentsToClose = New List(Of Document)
        If _frameSelection.Count > 5 Then Progressbar = New ProgressShow("ProcessingDocuments".Translate, _frameSelection.Count)
        Dim files = _frameSelection.Keys.ToList
        For Each file In files
            If Progressbar?.CancelPressed Then Exit For

            Dim doc = DocumentsHelper.Open(file)
            If IsNothing(doc) Then Continue For

            If doc.WasClosed Then documentsToClose.Add(doc)
            Dim frameData = doc.FrameData.Copy
            frameData.AssignPrimaryKey("Num")
            Progressbar?.PerformStep("Processing".Translate(doc.GetFileName))
            SysVarHandler.SetVar(doc, "BACKGROUNDPLOT", 0)
            Dim plotSession = _frameSelection(file).Cut
            Using doc.LockDocument
                Dim sheet = 0
                Using plotEngine = PlotFactory.CreatePublishEngine()
                    Using plotProgressDialog = New PlotProgressDialog(False, plotSession.Count, True)
                        For Each number In plotSession
                            Dim frameRow = frameData.Rows.Find(number)
                            If IsNothing(frameRow) Then errorMessage.Add("Sheet {0} not found".NotYetTranslated(number)) : Continue For

                            LayoutManager.Current.CurrentLayout = frameRow.GetValue("LayoutName")
                            Dim plotSettings = WireUpPlotSettings(doc, frameRow)
                            If IsNothing(plotSettings) Then Continue For

                            Dim plotInfo = WireUpPlotInfo(plotSettings, frameRow)
                            sheet += 1
                            If sheet = 1 Then
                                InitializePlotProgressDialog(plotProgressDialog, plotSession.Count)
                                plotEngine.BeginPlot(plotProgressDialog, Nothing)
                            End If
                            Dim fileName = "{TempFolder}\_{0}".Compose(Randomizer.GetString(12))
                            plotEngine.BeginDocument(plotInfo, doc.GetFileName, Nothing, copies, _plotToFile, fileName)
                            output.Add("{0}.pdf".Compose(fileName))

                            Dim drawingSheetNumber = "{0}-{1}".Compose(frameRow.GetString("Drawing"), frameRow.GetString("Sheet"))
                            PerformPagePlotProgressDialog(plotProgressDialog, drawingSheetNumber, sheet, plotSession.Count)
                            ProcessingPage(plotEngine, plotInfo, plotProgressDialog, True, False)
                            plotEngine.EndDocument(Nothing)
                        Next
                        plotProgressDialog.OnEndPlot()
                        If sheet > 0 Then plotEngine.EndPlot(Nothing)
                    End Using
                End Using
            End Using
            If documentsToClose.Count < 10 Then Continue For

            DocumentsHelper.Close(documentsToClose.ToArray)
            documentsToClose.Clear()
        Next
        DocumentsHelper.Close(documentsToClose.ToArray)
        Progressbar?.Dispose()
        Progressbar = Nothing
        If errorMessage.Count > 0 Then MsgBox(String.Join(vbLf, errorMessage))
        Return output.ToArray
    End Function

    ''' <summary>
    ''' Plot the selected frames grouped by document.
    ''' </summary>
    ''' <param name="copies">Number of copies to plot.</param>
    ''' <returns>An array of fullnames of temporary files.</returns>
    Public Function PlotMultiPage(copies As Integer) As String()
        If Not PlotFactory.ProcessPlotState = ProcessPlotState.NotPlotting Then MessageBoxInfo("Another plot".Translate) : Return {}

        FileSystemHelper.CreateFolder("{TempFolder}".Compose)
        Dim output = New List(Of String)
        Dim errorMessage = New List(Of String)
        Dim documentsToClose = New List(Of Document)
        If _frameSelection.Count > 5 Then Progressbar = New ProgressShow("ProcessingDocuments".Translate, _frameSelection.Count)
        Dim files = _frameSelection.Keys.ToList
        For Each file In files
            If Progressbar?.CancelPressed Then Exit For

            Dim doc = DocumentsHelper.Open(file)
            If IsNothing(doc) Then Continue For

            If doc.WasClosed Then documentsToClose.Add(doc)
            Progressbar?.PerformStep("Processing".Translate(doc.GetFileName))
            Dim frameData = doc.FrameData.Copy
            frameData.AssignPrimaryKey("Num")
            Dim plotSessions = GetPlotSessions(file, frameData)
            SysVarHandler.SetVar(doc, "BACKGROUNDPLOT", 0)
            Using doc.LockDocument
                For Each plotSession In plotSessions
                    If plotSession.Count = 0 Then Continue For

                    Dim sheet = 0
                    Using plotEngine = PlotFactory.CreatePublishEngine()
                        Using plotProgressDialog = New PlotProgressDialog(False, plotSession.Count, True)
                            For Each number In plotSession
                                Dim frameRow = frameData.Rows.Find(number)
                                If IsNothing(frameRow) Then errorMessage.Add("Sheet {0} not found".NotYetTranslated(number)) : Continue For

                                LayoutManager.Current.CurrentLayout = frameRow.GetValue("LayoutName")
                                Dim plotSettings = WireUpPlotSettings(doc, frameRow)
                                If IsNothing(plotSettings) Then Continue For

                                Dim plotInfo = WireUpPlotInfo(plotSettings, frameRow)
                                sheet += 1
                                If sheet = 1 Then
                                    InitializePlotProgressDialog(plotProgressDialog, plotSession.Count)
                                    plotEngine.BeginPlot(plotProgressDialog, Nothing)
                                    Dim fileName = "{TempFolder}\_{1}".Compose(Randomizer.GetString(12))
                                    plotEngine.BeginDocument(plotInfo, doc.GetFileName, Nothing, copies, _plotToFile, fileName)
                                    output.Add("{0}.pdf".Compose(fileName))
                                End If
                                Dim drawingSheetNumber = "{0}-{1}".Compose(frameRow.GetString("Drawing"), frameRow.GetString("Sheet"))
                                PerformPagePlotProgressDialog(plotProgressDialog, drawingSheetNumber, sheet, plotSession.Count)
                                ProcessingPage(plotEngine, plotInfo, plotProgressDialog, sheet = plotSession.Count, False)
                            Next
                            If sheet > 0 Then plotEngine.EndDocument(Nothing)
                            plotProgressDialog.OnEndPlot()
                            If sheet > 0 Then plotEngine.EndPlot(Nothing)
                        End Using
                    End Using
                Next
            End Using
            If documentsToClose.Count < 10 Then Continue For

            DocumentsHelper.Close(documentsToClose.ToArray)
            documentsToClose.Clear()
        Next
        DocumentsHelper.Close(documentsToClose.ToArray)
        Progressbar?.Dispose()
        Progressbar = Nothing
        If errorMessage.Count > 0 Then MsgBox(String.Join(vbLf, errorMessage))
        Return output.ToArray
    End Function

    ''' <summary>
    ''' Creates a plotpreview for the first selected frame.
    ''' </summary>
    ''' <returns><see cref="Autodesk.AutoCAD.PlottingServices.PreviewEndPlotStatus"/></returns>
    Public Function PlotPreview() As PreviewEndPlotStatus
        If Not PlotFactory.ProcessPlotState = ProcessPlotState.NotPlotting Then MessageBoxInfo("Another plot".Translate) : Return PreviewEndPlotStatus.Cancel

        Dim documentsToClose = New List(Of Document)
        Dim file = _frameSelection.Keys(0)
        Dim num = _frameSelection(file).Cut.Item(0)
        Dim doc = DocumentsHelper.Open(file)
        If IsNothing(doc) Then Return PreviewEndPlotStatus.Cancel

        If doc.WasClosed Then documentsToClose.Add(doc)
        Dim frameData = doc.FrameData.Copy
        frameData.AssignPrimaryKey("Num")
        Application.SetSystemVariable("BACKGROUNDPLOT", 0)
        Dim output = Autodesk.AutoCAD.PlottingServices.PreviewEndPlotStatus.Cancel
        Using doc.LockDocument
            Using plotEngine = PlotFactory.CreatePreviewEngine(PreviewEngineFlags.Plot)
                Using plotProgressDialog = New PlotProgressDialog(True, 1, True)
                    Dim frameRow = frameData.Rows.Find(num)
                    If IsNothing(frameRow) Then DocumentsHelper.Close(documentsToClose.ToArray) : Return PreviewEndPlotStatus.Cancel

                    LayoutManager.Current.CurrentLayout = frameRow.GetValue("LayoutName")
                    Dim plotSettings = WireUpPlotSettings(doc, frameRow)
                    If IsNothing(plotSettings) Then DocumentsHelper.Close(documentsToClose.ToArray) : Return PreviewEndPlotStatus.Cancel

                    Dim plotInfo = WireUpPlotInfo(plotSettings, frameRow)
                    InitializePlotProgressDialog(plotProgressDialog, 1)
                    plotEngine.BeginPlot(plotProgressDialog, Nothing)
                    Dim fileName = "{0}\_{1}".Compose(If(doc.GetPath = "", "{Desktop}".Compose, doc.GetPath), Randomizer.GetString(12))
                    plotEngine.BeginDocument(plotInfo, IO.Path.GetFileName(fileName), Nothing, 1, False, fileName)
                    Dim drawingSheetNumber = "{0}-{1}".Compose(frameRow.GetString("Drawing"), frameRow.GetString("Sheet"))
                    PerformPagePlotProgressDialog(plotProgressDialog, drawingSheetNumber, 1, 1)
                    output = ProcessingPage(plotEngine, plotInfo, plotProgressDialog, True, True)
                    plotEngine.EndDocument(Nothing)
                    plotProgressDialog.OnEndPlot()
                    plotEngine.EndPlot(Nothing)
                End Using
            End Using
        End Using
        DocumentsHelper.Close(documentsToClose.ToArray)
        Return output
    End Function

    ''' <summary>
    ''' Sets the plotoptions for the selected frames.
    ''' </summary>
    Public Sub SetPlotLayout()
        Dim documentsToClose = New List(Of Document)
        If _frameSelection.Count > 5 Then Progressbar = New ProgressShow("ProcessingDocuments".Translate, _frameSelection.Count)
        Dim files = _frameSelection.Keys.ToList
        For Each file In files
            If Progressbar?.CancelPressed Then Exit For

            Dim doc = DocumentsHelper.Open(file)
            If IsNothing(doc) Then Continue For

            If doc.WasClosed Then documentsToClose.Add(doc)
            Dim frameData = doc.FrameData.Copy
            frameData.AssignPrimaryKey("Num")
            Progressbar?.PerformStep("Processing".Translate(doc.GetFileName))
            Using doc.LockDocument
                Dim plotSesion = _frameSelection(file).Cut
                For Each number In plotSesion
                    Dim frameRow = frameData.Rows.Find(number)
                    If IsNothing(frameRow) Then Continue For

                    LayoutManager.Current.CurrentLayout = frameRow.GetValue("LayoutName")
                    Dim plotSettings = WireUpPlotSettings(doc, frameRow)
                    If IsNothing(plotSettings) Then Continue For

                    Dim plotInfo = WireUpPlotInfo(plotSettings, frameRow)
                    Using tr = doc.Database.TransactionManager.StartTransaction()
                        Dim layout = tr.GetLayout(frameRow("LayoutID"), OpenMode.ForWrite)
                        layout.CopyFrom(plotSettings)
                        tr.Commit()
                    End Using
                    doc.Editor.Regen()
                Next
            End Using
            If documentsToClose.Count < 10 Then Continue For

            DocumentsHelper.Close(documentsToClose.ToArray, True)
            documentsToClose.Clear()
        Next
        DocumentsHelper.Close(documentsToClose.ToArray, True)
        Progressbar?.Dispose()
        Progressbar = Nothing
    End Sub

    'frameless

    ''' <summary>
    ''' Plots to the extents of the current layout (used when no frame is found) of the selected documents.
    ''' </summary>
    ''' <returns>An array of fullnames of temporary files.</returns>
    Public Function PlotFramelessDrawings() As String()
        If Not PlotFactory.ProcessPlotState = ProcessPlotState.NotPlotting Then MessageBoxInfo("Another plot".Translate) : Return {}

        FileSystemHelper.CreateFolder("{TempFolder}".Compose)
        Dim frameSizeData = DataSetHelper.LoadFromXml("{Support}\SetStandards.xml".Compose).GetTable("Frames", "Name")
        Dim frameSizes = frameSizeData.GetStringsFromColumn("Name")
        Dim dialog = New ListBoxDialog("SelectForm".Translate, frameSizes, "", "[Thin]".Translate)
        If Not dialog.DialogResult = System.Windows.Forms.DialogResult.OK Then Return {}

        Dim selectedItem = frameSizes(dialog.GetSelectedIndex)

        Dim plotOption = If(dialog.GetCheckState, _plotOption.Replace("Normal", "Thin"), _plotOption)

        Dim output = New List(Of String)
        Dim documentsToClose = New List(Of Document)
        If _frameSelection.Count > 5 Then Progressbar = New ProgressShow("Starting...".Translate, _frameSelection.Count)
        Dim files = _frameSelection.Keys.ToList
        For Each file In files
            If Progressbar?.CancelPressed Then Exit For

            Progressbar?.PerformStep("{0}{1}".Compose("Opening...".Translate, FileSystemHelper.LimitDisplayLengthFileName(file, 60)))
            Dim doc = DocumentsHelper.Open(file)
            If IsNothing(doc) Then Continue For

            Progressbar?.SetText("{0}{1}".Compose("Plotting...".Translate, FileSystemHelper.LimitDisplayLengthFileName(file, 60)))
            If doc.WasClosed Then documentsToClose.Add(doc)
            Select Case plotOption
                Case "NormalToPLT", "ThinToPLT"
                    PlotFramelessDrawing(doc, selectedItem, plotOption, 1)
                Case "NormalToPDF", "ThinToPDF"
                    Dim fileName = PlotFramelessDrawing(doc, selectedItem, plotOption, 1)
                    If Not fileName = "" Then output.Add(fileName)
            End Select
            If documentsToClose.Count < 10 Then Continue For

            DocumentsHelper.Close(documentsToClose.ToArray)
            documentsToClose.Clear()
        Next
        DocumentsHelper.Close(documentsToClose.ToArray)
        Progressbar?.Dispose()
        Progressbar = Nothing
        Return output.ToArray
    End Function

    ''' <summary>
    ''' Plots to the extents of the current layout (used when no frame is found) of the selected documents.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="frameSize">The size of frame to plot.</param>
    ''' <param name="plotOption">The plotoption (name) to use.</param>
    ''' <param name="copies">Number of copies to plot.</param>
    ''' <returns>The fullname of a temporary file.</returns>
    Private Function PlotFramelessDrawing(document As Document, frameSize As String, plotOption As String, copies As Integer) As String
        Dim db = document.Database
        Dim departmentRow = _departmentData.Rows.Find(Registerizer.UserSetting("Department"))
        Dim optionRow = _optionData.Rows.Find({plotOption, frameSize})
        Dim deviceName = departmentRow.GetString(optionRow.GetString("Device"))
        Dim deviceRow = _deviceData.Rows.Find({deviceName, optionRow.GetString("PaperSize")})
        _plotToFile = (optionRow.GetString("ToFile") = "Yes")
        Application.SetSystemVariable("BACKGROUNDPLOT", 0)
        Using document.LockDocument
            Using plotEngine = PlotFactory.CreatePublishEngine()
                Using plotProgressDialog = New PlotProgressDialog(False, 1, True)
                    Dim LayoutManager = Autodesk.AutoCAD.DatabaseServices.LayoutManager.Current
                    Dim layoutId = LayoutManager.GetLayoutId(LayoutManager.CurrentLayout)

                    'Get plotsettings form the layout
                    Dim layout = LayoutHelper.GetLayoutById(document.Database, layoutId)
                    Dim plotSettings = New PlotSettings(layout.ModelType)
                    plotSettings.CopyFrom(layout)
                    Dim plotExtents = If(layout.LayoutName = "Model", New Extents3d(db.Extmin, db.Extmax), layout.Extents)
                    Dim paperName = deviceRow.GetString("PaperName")
                    Dim rotation = GetRotation(plotExtents, deviceRow.GetString("Rotation"))
                    Dim centerPlot = deviceRow.GetString("CenterPlot")
                    Dim scale = optionRow.GetString("Scale")
                    Dim mediaName = PlotMediaHelper.GetLocaleMediaName(deviceName, paperName)
                    If mediaName = "" Then mediaName = PlotMediaHelper.GetLocaleMediaName(plotSettings, deviceName, paperName)
                    If mediaName = "" Then Return ""

                    'Setting the device and media configuration
                    Dim plotSettingsValidator = Autodesk.AutoCAD.DatabaseServices.PlotSettingsValidator.Current
                    plotSettingsValidator.RefreshLists(plotSettings)
                    If Not plotSettings.PlotConfigurationName = deviceName Or Not plotSettings.CanonicalMediaName = mediaName Then
                        Try
                            plotSettingsValidator.SetPlotConfigurationName(plotSettings, deviceName, mediaName)
                        Catch ex As Exception
                            MsgBox("Papername seems not available, but you can try again".NotYetTranslated(paperName, deviceName))
                            PlotMediaHelper.ResetLocaleMediaData(deviceName)
                            Return ""
                        End Try
                    End If
                    'Setting properties using the validator.
                    plotSettingsValidator.SetPlotPaperUnits(plotSettings, PlotPaperUnit.Millimeters)
                    plotSettingsValidator.SetPlotType(plotSettings, Autodesk.AutoCAD.DatabaseServices.PlotType.Extents)
                    plotSettingsValidator.SetUseStandardScale(plotSettings, scale = "Fit")
                    plotSettingsValidator.SetPlotRotation(plotSettings, rotation)
                    Select Case scale = "Fit"
                        Case True : plotSettingsValidator.SetStdScaleType(plotSettings, StdScaleType.ScaleToFit)
                        Case Else : plotSettingsValidator.SetCustomPrintScale(plotSettings, GetScale(scale, 1.0))
                    End Select
                    Select Case centerPlot = "Yes"
                        Case True : plotSettingsValidator.SetPlotCentered(plotSettings, True)
                        Case Else : plotSettingsValidator.SetPlotCentered(plotSettings, False) : plotSettingsValidator.SetPlotOrigin(plotSettings, New Point2d(0, 0))
                    End Select
                    Dim plotStyleSheetList = plotSettingsValidator.GetPlotStyleSheetList()
                    Select Case db.PlotStyleMode
                        Case True : plotSettingsValidator.SetCurrentStyleSheet(plotSettings, departmentRow.GetString(optionRow.GetString("Style")))
                        Case Else : plotSettingsValidator.SetCurrentStyleSheet(plotSettings, "acad.stb")
                    End Select
                    'Setting properties
                    plotSettings.ShadePlot = PlotSettingsShadePlotType.AsDisplayed
                    plotSettings.ShadePlotResLevel = ShadePlotResLevel.Normal
                    plotSettings.PrintLineweights = True
                    plotSettings.PlotTransparency = False
                    plotSettings.PlotPlotStyles = True
                    plotSettings.DrawViewportsFirst = True

                    Dim plotInfo = New PlotInfo With {.Layout = layoutId, .OverrideSettings = plotSettings}
                    Dim plotInfoValidator = New PlotInfoValidator With {.MediaMatchingPolicy = MatchingPolicy.MatchEnabled}
                    plotInfoValidator.Validate(plotInfo)

                    InitializePlotProgressDialog(plotProgressDialog, 1)
                    plotEngine.BeginPlot(plotProgressDialog, Nothing)
                    Dim output = "{TempFolder}\_{0}.pdf".Compose(Randomizer.GetString(12))
                    plotEngine.BeginDocument(plotInfo, IO.Path.GetFileName(document.Name), Nothing, copies, _plotToFile, output)
                    plotProgressDialog.PlotProgressPos = 1
                    plotProgressDialog.PlotMsgString(PlotMessageIndex.SheetSetProgressCaption) = "Progress".Translate(1, 1)
                    plotProgressDialog.PlotMsgString(PlotMessageIndex.SheetProgressCaption) = "Plotting".Translate(LayoutManager.CurrentLayout)
                    ProcessingPage(plotEngine, plotInfo, plotProgressDialog, True, False)
                    plotEngine.EndDocument(Nothing)
                    plotProgressDialog.OnEndPlot()
                    plotEngine.EndPlot(Nothing)
                    Return output
                End Using
            End Using
        End Using
    End Function

    'private subs

    ''' <summary>
    ''' Sets the current camera view to zero to avoid an offset during plotting.
    ''' </summary>
    ''' <param name="editor">The present editor.</param>
    Private Sub SetCurrentViewToZero(editor As Editor)
        Dim currentView = editor.GetCurrentView
        Dim isDirty = False
        If Not currentView.Target = New Point3d(0, 0, 0) Then currentView.Target = New Point3d(0, 0, 0) : isDirty = True
        If Not currentView.ViewDirection = New Vector3d(0, 0, 1) Then currentView.ViewDirection = New Vector3d(0, 0, 1) : isDirty = True
        If isDirty Then editor.SetCurrentView(currentView)
    End Sub

    ''' <summary>
    ''' Initializes the plotprogress dialogbox.
    ''' </summary>
    ''' <param name="plotProgressDialog">Dialogbox to initialize.</param>
    ''' <param name="frames">Number of frames to plot.</param>
    Private Sub InitializePlotProgressDialog(ByRef plotProgressDialog As PlotProgressDialog, frames As Integer)
        plotProgressDialog.PlotMsgString(PlotMessageIndex.DialogTitle) = Registerizer.GetApplicationVersion()
        plotProgressDialog.PlotMsgString(PlotMessageIndex.CancelJobButtonMessage) = "Cancel Job".Translate
        plotProgressDialog.PlotMsgString(PlotMessageIndex.CancelSheetButtonMessage) = "Cancel Sht".Translate
        plotProgressDialog.LowerPlotProgressRange = 0
        plotProgressDialog.UpperPlotProgressRange = frames
        plotProgressDialog.LowerSheetProgressRange = 0
        plotProgressDialog.UpperSheetProgressRange = 100
        plotProgressDialog.PlotProgressPos = 0
        plotProgressDialog.OnBeginPlot()
        plotProgressDialog.IsVisible = True
    End Sub

    ''' <summary>
    ''' Perform step in the plotprogress dialogbox.
    ''' </summary>
    ''' <param name="plotProgressDialog">Dialogbox to use.</param>
    ''' <param name="sheetProgressCaption">The caption of sheetprogress.</param>
    ''' <param name="status">Progress status.</param>
    ''' <param name="total">Number of sheets.</param>
    Private Sub PerformPagePlotProgressDialog(ByRef plotProgressDialog As PlotProgressDialog, sheetProgressCaption As String, status As Integer, total As Integer)
        plotProgressDialog.PlotProgressPos = status
        plotProgressDialog.PlotMsgString(PlotMessageIndex.SheetSetProgressCaption) = "Progress".Translate(status, total)
        plotProgressDialog.PlotMsgString(PlotMessageIndex.SheetProgressCaption) = "Plotting".Translate(sheetProgressCaption)
    End Sub

    'private functions

    ''' <summary>
    ''' Splits the plotting session if different rotations are needed.
    ''' </summary>
    ''' <param name="file">The fullname of the document to be examined.</param>
    ''' <param name="frameData">The frame database of the current document.</param>
    ''' <returns>An array of plotsession lists</returns>
    Private Function GetPlotSessions(file As String, frameData As Data.DataTable) As List(Of String)()
        Dim firstPlotSesion = New List(Of String)
        Dim secondPlotSesion = New List(Of String)
        Dim sheet = 1
        Dim rotation = 0
        Dim departmentRow = _departmentData.Rows.Find(Registerizer.UserSetting("Department"))
        For Each number In _frameSelection(file).Cut
            Dim frameRow = frameData.Rows.Find(number)
            If IsNothing(frameRow) Then Continue For

            Dim frameExtents = frameRow.GetExtents3d("BlockExtents")
            Dim optionRow = _optionData.Rows.Find({_plotOption, frameRow.GetString("FrameSize")})
            Dim deviceName = departmentRow.GetString(optionRow.GetString("Device"))
            Dim deviceRow = _deviceData.Rows.Find({deviceName, optionRow.GetString("PaperSize")})
            Select Case True
                Case sheet = 1 : rotation = GetRotation(frameExtents, deviceRow.GetString("Rotation")) : firstPlotSesion.Add(number)
                Case rotation = GetRotation(frameExtents, deviceRow.GetString("Rotation")) : firstPlotSesion.Add(number)
                Case Else : secondPlotSesion.Add(number)
            End Select
            sheet += 1
        Next
        Return {firstPlotSesion, secondPlotSesion}
    End Function

    ''' <summary>
    ''' Wires up the plotsettings for the specified frame (-record).
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="frameRow">The framerecord.</param>
    ''' <returns>A <see cref="PlotSettings"/> object.</returns>
    Private Function WireUpPlotSettings(document As Document, frameRow As DataRow) As PlotSettings
        SetCurrentViewToZero(document.Editor)
        Return WireUpPlotSettings(document.Database, frameRow)
    End Function

    ''' <summary>
    ''' Wires up the plotsettings for the specified frame (-record).
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="frameRow">The framerecord.</param>
    ''' <returns>A <see cref="PlotSettings"/> object.</returns>
    Private Function WireUpPlotSettings(database As Database, frameRow As DataRow) As PlotSettings
        Dim departmentRow = _departmentData.Rows.Find(Registerizer.UserSetting("Department"))
        Dim optionRow = _optionData.Rows.Find({_plotOption, frameRow.GetString("FrameSize")})
        Dim deviceName = departmentRow.GetString(optionRow.GetString("Device"))
        Dim deviceRow = _deviceData.Rows.Find({deviceName, optionRow.GetString("PaperSize")})
        If IsNothing(deviceRow) Then
            MsgBox("Papersize not available".Translate(optionRow.GetString("PaperSize"), deviceName))
            Return Nothing
        End If

        _plotToFile = (optionRow.GetString("ToFile") = "Yes")
        Dim frameExtents = frameRow.GetExtents3d("BlockExtents")
        Dim paperName = deviceRow.GetString("PaperName")
        Dim rotation = GetRotation(frameExtents, deviceRow.GetString("Rotation"))
        Dim centerPlot = deviceRow.GetString("CenterPlot")
        Dim scale = optionRow.GetString("Scale")

        'Get plotsettings form the layout
        Dim layout = LayoutHelper.GetLayoutById(database, frameRow.GetValue("LayoutID"))
        Dim output = New PlotSettings(layout.ModelType)
        output.CopyFrom(layout)
        Dim mediaName = PlotMediaHelper.GetLocaleMediaName(deviceName, paperName)
        If mediaName = "" Then mediaName = PlotMediaHelper.GetLocaleMediaName(output, deviceName, paperName)
        If mediaName = "" Then Return Nothing

        'Setting the device and media configuration
        Dim plotSettingsValidator = Autodesk.AutoCAD.DatabaseServices.PlotSettingsValidator.Current
        plotSettingsValidator.RefreshLists(output)
        If Not output.PlotConfigurationName = deviceName Or Not output.CanonicalMediaName = mediaName Then
            Try
                plotSettingsValidator.SetPlotConfigurationName(output, deviceName, mediaName)
            Catch ex As Exception
                MsgBox("Papername seems not available, but you can try again".NotYetTranslated(paperName, deviceName))
                PlotMediaHelper.ResetLocaleMediaData(deviceName)
                Return Nothing
            End Try
        End If
        'Setting properties using the validator.
        If Not output.PlotPaperUnits = PlotPaperUnit.Millimeters Then plotSettingsValidator.SetPlotPaperUnits(output, PlotPaperUnit.Millimeters)
        plotSettingsValidator.SetPlotWindowArea(output, frameExtents.ConvertTo2d)
        plotSettingsValidator.SetPlotType(output, Autodesk.AutoCAD.DatabaseServices.PlotType.Window)
        plotSettingsValidator.SetUseStandardScale(output, scale = "Fit")
        plotSettingsValidator.SetPlotRotation(output, rotation)
        Select Case scale = "Fit"
            Case True : plotSettingsValidator.SetStdScaleType(output, StdScaleType.ScaleToFit)
            Case Else : plotSettingsValidator.SetCustomPrintScale(output, GetScale(scale, frameRow("ScaleFactor")))
        End Select
        Select Case centerPlot = "Yes"
            Case True : plotSettingsValidator.SetPlotCentered(output, True)
            Case Else : plotSettingsValidator.SetPlotCentered(output, False) : plotSettingsValidator.SetPlotOrigin(output, New Point2d(0, 0))
        End Select
        Dim plotStyleSheetList = plotSettingsValidator.GetPlotStyleSheetList()
        Select Case database.PlotStyleMode
            Case True : plotSettingsValidator.SetCurrentStyleSheet(output, departmentRow.GetString(optionRow.GetString("Style")))
            Case Else : plotSettingsValidator.SetCurrentStyleSheet(output, "acad.stb")
        End Select
        'Setting properties
        output.ShadePlot = PlotSettingsShadePlotType.AsDisplayed
        output.ShadePlotResLevel = ShadePlotResLevel.Normal
        output.PrintLineweights = True
        output.PlotTransparency = False
        output.PlotPlotStyles = True
        output.DrawViewportsFirst = True
        Return output
    End Function

    ''' <summary>
    ''' Wires up the plotinfo for the <see cref="PlotSettings"/> object.
    ''' </summary>
    ''' <param name="plotSettings"></param>
    ''' <param name="frameRow">The framerecord.</param>
    ''' <returns>A <see cref="PlotInfo"/> object.</returns>
    Private Function WireUpPlotInfo(plotSettings As PlotSettings, frameRow As DataRow) As PlotInfo
        Dim plotInfo = New PlotInfo With {.Layout = frameRow("LayoutID"), .OverrideSettings = plotSettings}
        Dim plotInfoValidator = New PlotInfoValidator With {.MediaMatchingPolicy = MatchingPolicy.MatchEnabled}
        plotInfoValidator.Validate(plotInfo)
        Return plotInfo
    End Function

    ''' <summary>
    ''' Generates the graphics for the page to be plotted.
    ''' </summary>
    ''' <param name="plotEngine">The current <see cref="PlotEngine"/>.</param>
    ''' <param name="plotInfo">The current <see cref="PlotInfo"/> object.</param>
    ''' <param name="plotProgressDialog">The current plotprogress dialogbox.</param>
    ''' <param name="lastPage">Specify if it is the last page.</param>
    ''' <param name="preview">Specify the preview mode.</param>
    ''' <returns>A <see cref="PreviewEndPlotStatus"/> object (for preview mode).</returns>
    Private Function ProcessingPage(plotEngine As PlotEngine, plotInfo As PlotInfo, plotProgressDialog As PlotProgressDialog, lastPage As Boolean, preview As Boolean) As PreviewEndPlotStatus
        Dim output = PreviewEndPlotStatus.Cancel
        plotProgressDialog.OnBeginSheet()
        plotProgressDialog.SheetProgressPos = 0
        Dim pageInfo = New PlotPageInfo()
        plotEngine.BeginPage(pageInfo, plotInfo, lastPage, Nothing)
        plotEngine.BeginGenerateGraphics(Nothing)
        plotProgressDialog.SheetProgressPos = 50
        plotEngine.EndGenerateGraphics(Nothing)
        Select Case preview
            Case True
                Dim previewEndPlotInfo = New PreviewEndPlotInfo
                plotEngine.EndPage(previewEndPlotInfo)
                output = previewEndPlotInfo.Status
            Case Else
                plotEngine.EndPage(Nothing)
        End Select
        plotProgressDialog.SheetProgressPos = 100
        plotProgressDialog.OnEndSheet()
        Return output
    End Function

    ''' <summary>
    ''' Gets a <see cref="PlotRotation"/> depending on the frame ratio and the preset rotation.
    ''' </summary>
    ''' <param name="plotExtents"></param>
    ''' <param name="rotation"></param>
    ''' <returns>The <see cref="PlotRotation"/>.</returns>
    Private Function GetRotation(plotExtents As Extents3d, rotation As String) As PlotRotation
        Dim distanceX = plotExtents.MaxPoint.X - plotExtents.MinPoint.X
        Dim distanceY = plotExtents.MaxPoint.Y - plotExtents.MinPoint.Y
        Select Case If(distanceX > distanceY, "{0}_L".Compose(rotation), rotation)
            Case "000" : Return PlotRotation.Degrees000
            Case "090" : Return PlotRotation.Degrees090
            Case "180" : Return PlotRotation.Degrees180
            Case "270" : Return PlotRotation.Degrees270
            Case "000_L" : Return PlotRotation.Degrees090
            Case "090_L" : Return PlotRotation.Degrees180
            Case "180_L" : Return PlotRotation.Degrees270
            Case "270_L" : Return PlotRotation.Degrees000
            Case Else : Return PlotRotation.Degrees000
        End Select
    End Function

    ''' <summary>
    ''' Gets a <see cref="CustomScale"/> depending on the preset scale and scale factor.
    ''' </summary>
    ''' <param name="presetScale">The preset scale.</param>
    ''' <param name="scaleFactor">The scale factor.</param>
    ''' <returns>The <see cref="CustomScale"/>.</returns>
    Private Function GetScale(presetScale As String, scaleFactor As Double) As CustomScale
        Select Case presetScale = "Org"
            Case True : Return New CustomScale(1, scaleFactor)
            Case Else : Return New CustomScale(1, scaleFactor * presetScale.ToDouble)
        End Select
    End Function

End Class
