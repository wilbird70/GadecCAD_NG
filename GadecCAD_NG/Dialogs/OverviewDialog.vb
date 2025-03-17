'Gadec Engineerings Software (c) 2022
Imports System.Data
Imports System.Windows.Forms
Imports GadecCAD.Commands
Imports GadecCAD_NG.Extensions

''' <summary>
''' <para><see cref="OverviewDialog"/> provides an overview of all the frames in the current project (folder).</para>
''' <para>The overview is like the <see cref="FramesPalette"/>, but with all header data available to view.</para>
''' </summary>
Public Class OverviewDialog
    ''' <summary>
    ''' The length of the drawingnumber prefix to group into.
    ''' </summary>
    ''' <returns>The length.</returns>
    Public ReadOnly Property GroupPrefixLength As Integer

    ''' <summary>
    ''' The selected record.
    ''' </summary>
    Private _selectedRow As DataRow
    ''' <summary>
    ''' The index of the selected record.
    ''' </summary>
    Private _selectedRowIndex As Integer = -1
    ''' <summary>
    ''' Contains a list of keys of the groups.
    ''' </summary>
    Private _groupKeys As List(Of String)
    ''' <summary>
    ''' The present framelist database.
    ''' </summary>
    Private _frameListData As DataTable
    ''' <summary>
    ''' Contains a list of associate xml-files.
    ''' </summary>
    Private _xmlFiles As New List(Of String)
    ''' <summary>
    ''' A timer to delay the display of the image/text balloon.
    ''' </summary>
    Private _mouseHoverTimer As Timer
    ''' <summary>
    ''' Contains the current/previous height of the dialogbox.
    ''' </summary>
    Private _formHeight As Integer
    ''' <summary>
    ''' Contains the current/previous width of the dialogbox.
    ''' </summary>
    Private _formWidth As Integer
    ''' <summary>
    ''' Determines if this dialogbox is fully (re-)loaded.
    ''' </summary>
    Private _dialogLoaded As Boolean = False

    ''' <summary>
    ''' The present folder.
    ''' </summary>
    Private ReadOnly _folder As String
    ''' <summary>
    ''' The tooltip to use on this palette.
    ''' </summary>
    Private ReadOnly _toolTip As New ToolTip With {.AutomaticDelay = 200}
    ''' <summary>
    ''' Determines if this dialogbox is started from the <see cref="HeaderDialog"/>.
    ''' </summary>
    Private ReadOnly _fromEditHeader As Boolean

    'form

    ''' <summary>
    ''' Initializes a new instance of <see cref="OverviewDialog"/> with the specified properties.
    ''' <para><see cref="OverviewDialog"/> provides an overview of all the frames in the current project (folder).</para>
    ''' <para>The overview is like the <see cref="FramesPalette"/>, but with all header data available to view.</para>
    ''' </summary>
    ''' <param name="fromHeaderDialog">True if started from the <see cref="HeaderDialog"/>.</param>
    Sub New(fromHeaderDialog As Boolean)
        ' This call is required by the designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.

        Me.Text = Registerizer.GetApplicationVersion()
        Translator.TranslateControls(Me)

        Dim doc = ActiveDocument()
        _folder = doc.GetPath

        Dim result = FirstCommands.UpdateDrawingList(doc.Name, True)

        Dim frameSetController = New FrameSetHandler(doc.Name, False)
        _frameListData = frameSetController.UpdatedFrameListData
        _fromEditHeader = fromHeaderDialog
        _GroupPrefixLength = {PaletteHelper.GetPrefixLength, 0}.Max
        _formHeight = Me.Height
        _formWidth = Me.Width
        _dialogLoaded = True

        _toolTip.SetToolTip(ViewDescriptionButton, "ltDescr".Translate)
        _toolTip.SetToolTip(ViewClientButton, "ltClient".Translate)
        _toolTip.SetToolTip(ViewProjectButton, "ltProject".Translate)
        _toolTip.SetToolTip(ViewDesignButton, "ltDesign".Translate)
        _toolTip.SetToolTip(ViewRevisionButton, "Revision".Translate)

        Me.Width = {840, ApplicationHelper.WindowSize.Width - 35}.Min
        Me.Height = {500, ApplicationHelper.WindowSize.Height - 70}.Min
        Me.ShowDialog()
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when user clicks on an empty part of this dialog.
    ''' <para>If screenshots are enabled (by doubleclicking on the languageflag, see <see cref="SettingsPalette"/>) a screenshot of this dialog will be taken.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Me_Click(sender As Object, e As EventArgs) Handles Me.Click
        Try
            If ScreenShot Then ImageHelper.GetScreenShot(Me.Location, Me.Size)
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the dialogbox is loading.
    ''' <para>It changes the size and position of controls depending on started from the <see cref="FramesPalette"/> or the <see cref="HeaderDialog"/>.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Me_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            AssociateFoldersListBox.Items.Add("MoreFolders".Translate)
            Cursor.Current = Cursors.WaitCursor
            WireUpGrouping()

            Dim menuData = DataSetHelper.LoadFromXml("{Support}\SetContextMenus.xml".Compose)
            Select Case _fromEditHeader
                Case True
                    ltClose.Visible = False
                    ltSelectAll.Visible = False
                    Me.CancelButton = ltCancel
                    UpButton.Top -= 24
                    DownButton.Top -= 24
                    GroupingLabel.Top -= 24
                Case Else
                    GroupsListBox.Left = 12
                    ltSelectAll.Left = 162
                    ltSelectAll.Width += 228
                    UpButton.Left = 162
                    DownButton.Left = 162
                    GroupingLabel.Left = 162
                    AssociateFoldersListBox.Visible = False
                    ltOK.Visible = False
                    ltCancel.Visible = False
                    Me.CancelButton = ltClose
                    Dim contextMenuStrip = MenuStripHelper.Create(menuData.GetTable("Drawinglist"), AddressOf ContextMenuStripClickEventHandler, My.Resources.ResourceManager)
                    AddHandler contextMenuStrip.VisibleChanged, AddressOf ContextMenuStripVisibleChangedEventHandler
                    FramesDataGridView.ContextMenuStrip = contextMenuStrip
                    ltSelectAll.ContextMenuStrip = contextMenuStrip
            End Select
            _dialogLoaded = True

            Dim files = IO.Directory.GetFiles(_folder, "*.dwl2")
            Dim message = ""
            For Each file In files
                Dim dwgInUse = DocumentsHelper.GetInuseInfo(file)
                If dwgInUse?.ByOtherUser Then
                    message &= "DwgInUse".Translate(IO.Path.GetFileNameWithoutExtension(file), dwgInUse.UserName, dwgInUse.MachineName, dwgInUse.TimeString)
                End If
            Next
            Cursor.Current = Cursors.Default
            If Not message = "" Then MsgBox("MsgInUse".Translate(message))
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user resizes this dialogbox.
    ''' <para>It handles the size and position of controls it contains.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Me_SizeChanged(sender As Object, e As EventArgs) Handles Me.SizeChanged
        Try
            If Not _dialogLoaded Then Exit Sub

            Me.Height = {Me.Height, 320}.Max
            Me.Width = {Me.Width, 600}.Max
            Dim heightDifference = Me.Height - _formHeight
            If Not heightDifference = 0 Then
                ltOK.Top += heightDifference
                ltCancel.Top += heightDifference
                ltClose.Top += heightDifference
                FramesDataGridView.Height += heightDifference
                GroupsListBox.Top += heightDifference
                UpButton.Top += heightDifference
                DownButton.Top += heightDifference
                GroupingLabel.Top += heightDifference
                ltSelectAll.Top += heightDifference

                ViewDescriptionButton.Top += heightDifference
                ViewClientButton.Top += heightDifference
                ViewProjectButton.Top += heightDifference
                ViewDesignButton.Top += heightDifference
                ViewRevisionButton.Top += heightDifference

                AssociateFoldersListBox.Top += heightDifference
            End If
            Dim widthDifference = Me.Width - _formWidth
            If Not widthDifference = 0 Then
                FramesDataGridView.Width += widthDifference
                ltSelectAll.Width += widthDifference
                ltCancel.Left += widthDifference
                ltClose.Left += widthDifference
                ltOK.Left += widthDifference
                ViewDescriptionButton.Left += widthDifference
                ViewClientButton.Left += widthDifference
                ViewProjectButton.Left += widthDifference
                ViewDesignButton.Left += widthDifference
                ViewRevisionButton.Left += widthDifference
            End If
            _formHeight = Me.Height
            _formWidth = Me.Width
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    'functions

    ''' <summary>
    ''' Gets the selected record.
    ''' </summary>
    ''' <returns>The record.</returns>
    Function GetSelectedRow() As DataRow
        Return _selectedRow
    End Function

    'buttons

    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks the OK button.
    ''' <para>It sets the buttonvalue, points the selected record and closes the dialogbox.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub AcceptButton_Click(sender As Object, e As EventArgs) Handles ltOK.Click
        Try
            If FramesDataGridView.SelectedRows.Count = 1 Then
                Select Case IsNothing(FramesDataGridView.CurrentRow)
                    Case True : _selectedRow = DirectCast(FramesDataGridView.Rows(0).DataBoundItem, DataRowView).Row
                    Case Else : _selectedRow = DirectCast(FramesDataGridView.CurrentRow.DataBoundItem, DataRowView).Row
                End Select
                Me.Hide()
            End If
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks the Cancel button.
    ''' <para>It sets the buttonvalue and closes the dialogbox.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub CancelButton_Click(sender As Object, e As EventArgs) Handles ltCancel.Click
        Try
            Me.Hide()
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks the Up button.
    ''' <para>It increases the length of the group prefix.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub UpButton_Click(sender As Object, e As EventArgs) Handles UpButton.Click
        Try
            _GroupPrefixLength = {_GroupPrefixLength + 1, 10}.Min
            WireUpGrouping()
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks the Down button.
    ''' <para>It decreases the length of the group prefix.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub DownButton_Click(sender As Object, e As EventArgs) Handles DownButton.Click
        Try
            _GroupPrefixLength = {_GroupPrefixLength - 1, 0}.Max
            WireUpGrouping()
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user left- or rightclicks the SelectAll button.
    ''' <para>It selects all the frames in the framelist.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub SelectAllButton_MouseDown(sender As Object, e As MouseEventArgs) Handles ltSelectAll.MouseDown
        Try
            If e.Button = MouseButtons.Right Or e.Button = MouseButtons.Left Then
                FramesDataGridView.SelectAll()
            End If
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks the description button.
    ''' <para>It displays the description range of data.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ViewDescriptionButton_Click(sender As Object, e As EventArgs) Handles ViewDescriptionButton.Click
        Registerizer.UserSetting("DrawinglistView", "2")
        LoadFramelist()
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks the client button.
    ''' <para>It displays the client range of data.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ViewClientButton_Click(sender As Object, e As EventArgs) Handles ViewClientButton.Click
        Registerizer.UserSetting("DrawinglistView", "3")
        LoadFramelist()
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks the project button.
    ''' <para>It displays the project range of data.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ViewProjectButton_Click(sender As Object, e As EventArgs) Handles ViewProjectButton.Click
        Registerizer.UserSetting("DrawinglistView", "4")
        LoadFramelist()
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks the design button.
    ''' <para>It displays the design range of data.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ViewDesignButton_Click(sender As Object, e As EventArgs) Handles ViewDesignButton.Click
        Registerizer.UserSetting("DrawinglistView", "5")
        LoadFramelist()
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks the revision button.
    ''' <para>It displays the revision range of data.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ViewRevisionButton_Click(sender As Object, e As EventArgs) Handles ViewRevisionButton.Click
        Registerizer.UserSetting("DrawinglistView", "6")
        LoadFramelist()
    End Sub

    'listboxes

    ''' <summary>
    ''' EventHandler for the event that occurs when the user changes the selected group.
    ''' <para>It loads the framelist with the selected group.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub GroupsListBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles GroupsListBox.SelectedIndexChanged
        Try
            LoadFramelist()
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user changes the selected associate folder.
    ''' <para>It loads the frame list of that particular folder.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub AssociateFoldersListBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles AssociateFoldersListBox.SelectedIndexChanged
        Try
            If Not _dialogLoaded OrElse AssociateFoldersListBox.SelectedIndex = -1 Then Exit Sub

            If AssociateFoldersListBox.SelectedIndex < _xmlFiles.Count Then
                Dim frameListData = DataSetHelper.LoadFromXml(_xmlFiles(AssociateFoldersListBox.SelectedIndex)).GetTable("Frames", "Filename;Num")
                _frameListData = FrameHelper.ConvertToFramelist(frameListData)
                WireUpGrouping()
            End If
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks the associate folder list.
    ''' <para>It will displays the associated folders, i.e. the subfolders of the parent folder of the current folder.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub AssociateFoldersListBox_Click(sender As Object, e As EventArgs) Handles AssociateFoldersListBox.Click
        Try
            If Not AssociateFoldersListBox.Items.Count = 1 Then Exit Sub

            AssociateFoldersListBox.Items.Clear()
            ltOK.Enabled = False
            ltCancel.Enabled = False
            Cursor.Current = Cursors.WaitCursor
            WireUpAssociateFolders()
            Cursor.Current = Cursors.Default
            ltOK.Enabled = True
            ltCancel.Enabled = True
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user moves the mouse over the associate folder list.
    ''' <para>It displays a tooltip.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub AssociateFoldersListBox_MouseMove(sender As Object, e As MouseEventArgs) Handles AssociateFoldersListBox.MouseMove
        Try
            Dim index = AssociateFoldersListBox.IndexFromPoint(e.Location)
            Select Case True
                Case index < 0
                Case index > AssociateFoldersListBox.Items.Count - 1
                Case _toolTip.GetToolTip(AssociateFoldersListBox) = AssociateFoldersListBox.Items(index).ToString
                Case Else : _toolTip.SetToolTip(AssociateFoldersListBox, AssociateFoldersListBox.Items(index).ToString())
            End Select
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    'datagridview

    ''' <summary>
    ''' EventHandler for the event that occurs when the user doubleclicks a cell of the framelist to select it.
    ''' <para>The document containing the frame will be opened and sets its view to the selected frame,</para>
    ''' <para>or the record will be selected for the editheader dialog, if derived from it.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub FramesDataGridView_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs) Handles FramesDataGridView.CellDoubleClick
        Try
            Me.DialogResult = DialogResult.OK
            Select Case _fromEditHeader
                Case True : AcceptButton_Click(Nothing, Nothing)
                Case Else : OpenDocument()
            End Select
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user hovers the mouse into a cell of the framelist.
    ''' <para>It highlights the row and starts a timer that displays the image/text balloon in a delayed manner.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub FramesDataGridView_CellMouseEnter(sender As Object, e As DataGridViewCellEventArgs) Handles FramesDataGridView.CellMouseEnter
        Try
            If NotNothing(_mouseHoverTimer) Then _mouseHoverTimer.Dispose() : _mouseHoverTimer = Nothing
            Select Case True
                Case Registerizer.UserSetting("HidePreviews") = "True"
                Case e.RowIndex < 0
                Case Else
                    _selectedRowIndex = e.RowIndex
                    _mouseHoverTimer = New Timer
                    AddHandler _mouseHoverTimer.Tick, AddressOf TimerTickEventHandler
                    _mouseHoverTimer.Interval = 350
                    _mouseHoverTimer.Enabled = True
                    FramesDataGridView.Rows(e.RowIndex).HighlightRow(True)
            End Select
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user hovers the mouse out of a cell of the framelist.
    ''' <para>It no longer highlights the row and stops the timer (for displaying the image/text balloon).</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub FramesDataGridView_CellMouseLeave(sender As Object, e As DataGridViewCellEventArgs) Handles FramesDataGridView.CellMouseLeave
        Try
            If NotNothing(_mouseHoverTimer) Then _mouseHoverTimer.Enabled = False
            Select Case True
                Case Registerizer.UserSetting("HidePreviews") = "True"
                Case e.RowIndex < 0
                Case FramesDataGridView.Columns.Contains("Num")
                    BalloonHelper.ShowFramePreview(Nothing, Nothing, Nothing)
                    FramesDataGridView.Rows(e.RowIndex).HighlightRow(False)
            End Select
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when columns added to the framelist.
    ''' <para>It sets the column to not sortable.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub FramesDataGridView_ColumnAdded(sender As Object, e As DataGridViewColumnEventArgs) Handles FramesDataGridView.ColumnAdded
        Try
            FramesDataGridView.Columns.Item(e.Column.Index).SortMode = DataGridViewColumnSortMode.NotSortable
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    'eventhandlers

    ''' <summary>
    ''' EventHandler for the event that occurs when the timer reaches the specified time.
    ''' <para>It displays the image/text balloon.</para>
    ''' </summary>
    Private Sub TimerTickEventHandler()
        Try
            _mouseHoverTimer.Enabled = False
            If _selectedRowIndex = -1 Or Not _selectedRowIndex < FramesDataGridView.Rows.Count Then Exit Sub

            Dim rowRectangle = FramesDataGridView.GetRowDisplayRectangle(_selectedRowIndex, True)
            Dim pointX = 0
            Dim pointY = rowRectangle.Y + FramesDataGridView.Top + (rowRectangle.Height / 2)
            Dim alignment As HorizontalAlignment = PaletteHelper.GetTitlebarLocation
            Select Case alignment = HorizontalAlignment.Left
                Case True : pointX = rowRectangle.X + FramesDataGridView.Left + FramesDataGridView.Width + 10
                Case Else : pointX = rowRectangle.X + FramesDataGridView.Left - 10
            End Select
            Dim displayPoint = PointToScreen(New Drawing.Point(pointX, pointY))
            If FramesDataGridView.Columns.Contains("Num") Then BalloonHelper.ShowFramePreview(FramesDataGridView.Rows(_selectedRowIndex), displayPoint, alignment)
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user selects an option in the contextmenu.
    ''' <para>It will executes the selected option.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ContextMenuStripClickEventHandler(sender As Object, e As EventArgs)
        Try
            Dim doc = ActiveDocument()
            Dim tag = DirectCast(sender, ToolStripItem).Tag.ToString
            Dim frameSelection = New Dictionary(Of String, String)
            For Each gridRow As DataGridViewRow In FramesDataGridView.Rows
                If gridRow.Selected Then
                    Dim file = "{0}\{1}".Compose(doc.GetPath, gridRow.Cells("Filename").Value.ToString)
                    Select Case frameSelection.ContainsKey(file)
                        Case True : frameSelection(file) &= ";{0}".Compose(gridRow.Cells("Num").Value)
                        Case Else : frameSelection.Add(file, gridRow.Cells("Num").Value)
                    End Select
                End If
            Next
            If FramesDataGridView.SelectedRows.Count > 0 Then
                Select Case tag
                    Case "EditH"
                        FrameHeaderHelper.EditHeader(doc, frameSelection, _frameListData)
                        Dim frameSetController = New FrameSetHandler(doc.Name, False)
                        _frameListData = frameSetController.UpdatedFrameListData
                        WireUpGrouping()
                    Case "AddRev"
                        FrameHeaderHelper.AddRevision(doc, frameSelection)
                        Dim frameSetController = New FrameSetHandler(doc.Name, False)
                        _frameListData = frameSetController.UpdatedFrameListData
                        WireUpGrouping()
                    Case "UpdateStatus"
                        FrameStampHelper.UpdateStatus(doc, frameSelection, _frameListData)
                        Dim frameSetController = New FrameSetHandler(doc.Name, False)
                        _frameListData = frameSetController.UpdatedFrameListData
                        WireUpGrouping()
                    Case "DwgList"
                        Dim fileName As String
                        Using drawingList = New DrawinglistCreator(frameSelection, _frameListData, _GroupPrefixLength)
                            If drawingList.NoSelection Then Exit Sub

                            Dim dialog = New CoverSheetDialog(drawingList.GetDrawinglistHeader)
                            If Not dialog.DialogResult = DialogResult.OK Then Exit Sub

                            drawingList.Create()
                            If drawingList.CreateFailure Then Exit Sub

                            Dim initialFileName = "{0}\{1}.dwg".Compose(doc.GetPath, drawingList.ProjectName)
                            fileName = FileSystemHelper.FileSaveAs(initialFileName)
                            If fileName = "" Then Exit Sub

                            drawingList.SaveAs(fileName)
                        End Using
                        Dim newDocument = DocumentsHelper.Open(fileName)
                        newDocument.ZoomExtents
                        Me.Hide()
                End Select
            End If
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the visibility of the contextmenu changes.
    ''' <para>If screenshots are enabled (by doubleclicking on the languageflag, see <see cref="SettingsPalette"/>) a screenshot of this contextmenu will be taken.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ContextMenuStripVisibleChangedEventHandler(sender As Object, e As EventArgs)
        Try
            If ScreenShot Then
                Dim toolStrip = TryCast(sender, ToolStrip)
                If Not toolStrip.Visible Then ImageHelper.GetScreenShot(toolStrip.Location, toolStrip.Size)
            End If
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    'private subs

    ''' <summary>
    ''' Opens the selected document.
    ''' </summary>
    Private Sub OpenDocument()
        If FramesDataGridView.SelectedRows.Count = 1 Then
            Me.Hide()
            Dim fileName = DirectCast(FramesDataGridView.Rows(FramesDataGridView.CurrentRow.Index).DataBoundItem, DataRowView).Row("Filename")
            DocumentsHelper.Open("{0}\{1}".Compose(_folder, fileName))
        End If
    End Sub

    ''' <summary>
    ''' Wires up the associated folders, i.e. the subfolders of the root of the current folder.
    ''' </summary>
    Private Sub WireUpAssociateFolders()
        Dim searchFolder = IO.Path.GetDirectoryName(_folder)
        _xmlFiles = IO.Directory.GetFiles(searchFolder, "Drawinglist.xml", IO.SearchOption.AllDirectories).ToSortedList
        Dim fileList = New List(Of String)
        Dim preSelect = 0
        Dim searchFolderLength = searchFolder.Length
        For i = 0 To _xmlFiles.Count - 1
            Dim file = _xmlFiles(i)
            Dim frameListData = DataSetHelper.LoadFromXml(file).GetTable("Frames")
            Dim rowCount = If(IsNothing(frameListData), "-", frameListData.Rows.Count.ToString)
            fileList.Add("..{0}   ({1})".Compose(file.EraseStart(searchFolderLength).EraseEnd(16), rowCount))
            If file = "{0}\Drawinglist.xml".Compose(_folder) Then preSelect = i
        Next
        _dialogLoaded = False
        AssociateFoldersListBox.Items.AddRange(fileList.ToArray)
        AssociateFoldersListBox.SetSelected(preSelect, True)
        _dialogLoaded = True
    End Sub

    ''' <summary>
    ''' Wires up the grouping of the frames.
    ''' </summary>
    Private Sub WireUpGrouping()
        Dim systems = New Dictionary(Of String, Integer)
        GroupingLabel.Text = _GroupPrefixLength
        GroupingLabel.Refresh()
        Select Case True
            Case _GroupPrefixLength = 0
                _groupKeys = {"*"}.ToList
                systems.Add("*", _frameListData.Rows.Count)
            Case Else
                For i = 0 To _frameListData.Rows.Count - 1
                    Dim system = "{0}*".Compose(_frameListData.Rows(i).GetString("Drawing").LeftString(_GroupPrefixLength))
                    If system.StartsWith("&") Then Continue For

                    Select Case systems.ContainsKey(system)
                        Case True : systems(system) += 1
                        Case Else : systems.Add(system, 1)
                    End Select
                Next
                _groupKeys = systems.Keys.ToSortedList
        End Select

        Dim texts = _groupKeys.Select(Function(key) "{0}   ({1})".Compose(key, systems(key)))
        GroupsListBox.Items.Clear()
        GroupsListBox.Items.AddRange(texts.ToArray)
        Select Case GroupsListBox.Items.Count > PaletteHelper.GetSelectedGroupIndex
            Case True : GroupsListBox.SetSelected(PaletteHelper.GetSelectedGroupIndex, True)
            Case Else : FramesDataGridView.Columns.Clear()
        End Select
    End Sub

    ''' <summary>
    ''' Loads the framelist and displays a selected range of data.
    ''' <para>The user can choose from description, customer, project, design and revision overview.</para>
    ''' </summary>
    Private Sub LoadFramelist()
        'kleur van knoppen verwijderen
        ViewDescriptionButton.UseVisualStyleBackColor = True
        ViewClientButton.UseVisualStyleBackColor = True
        ViewProjectButton.UseVisualStyleBackColor = True
        ViewDesignButton.UseVisualStyleBackColor = True
        ViewRevisionButton.UseVisualStyleBackColor = True
        Select Case Registerizer.UserSetting("DrawinglistView")
            Case "3" : ViewClientButton.BackColor = Drawing.Color.LightSkyBlue
            Case "4" : ViewProjectButton.BackColor = Drawing.Color.LightSkyBlue
            Case "5" : ViewDesignButton.BackColor = Drawing.Color.LightSkyBlue
            Case "6" : ViewRevisionButton.BackColor = Drawing.Color.LightSkyBlue
            Case Else : ViewDescriptionButton.BackColor = Drawing.Color.LightSkyBlue
        End Select
        'standaard opmaak
        Dim Font = FontHelper.ArialNarrowRegular
        FramesDataGridView.ColumnHeadersDefaultCellStyle.Font = Font
        FramesDataGridView.DefaultCellStyle.Font = Font
        FramesDataGridView.ScrollBars = ScrollBars.Vertical
        'data toewijzen
        _dialogLoaded = False
        Dim data = New DataView(_frameListData, "Drawing LIKE '{0}'".Compose(_groupKeys(GroupsListBox.SelectedIndex)), "Drawing,Sheet", DataViewRowState.CurrentRows).ToTable
        FramesDataGridView.DataSource = Nothing
        FramesDataGridView.DataSource = data
        'kolombreedten toewijzen
        Dim settings As String
        Select Case Registerizer.UserSetting("DrawinglistView")
            Case "3" : settings = "Dossier=*60;Drawing=*60;Sheet=*60;Client1=150;Client2=150;Client3=150;Client4=150"
            Case "4" : settings = "Dossier=*60;Drawing=*60;Sheet=*60;Project=120;Rev=120;Size=120;Scale=120;Design=120"
            Case "5" : settings = "Dossier=*60;Drawing=*60;Sheet=*60;Char=120;Date=120;Descr=120;Drawn=120;Check=120"
            Case "6" : settings = "Dossier=*60;Drawing=*60;Sheet=*60;LastRev_Char=120;LastRev_Date=120;LastRev_Descr=120;LastRev_Drawn=120;LastRev_Check=120"
            Case Else : settings = "Dossier=*60;Drawing=*60;Sheet=*60;Descr1=150;Descr2=150;Descr3=150;Descr4=150"
        End Select
        FramesDataGridView.SetColumnWidths(settings.Cut.ToIniDictionary)
        FramesDataGridView.AllowUserToResizeColumns = True
        _dialogLoaded = True
    End Sub

End Class