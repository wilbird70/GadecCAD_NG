'Gadec Engineerings Software (c) 2022
Imports System.Data
Imports System.Windows.Forms
Imports Autodesk.AutoCAD.Windows
Imports GadecCAD_NG.Extensions

''' <summary>
''' <para><see cref="FramesPalette"/> provides the design and functionality for the frames tab of the <see cref="PaletteSet"/>.</para>
''' </summary>
Public Class FramesPalette
    ''' <summary>
    ''' The length of the drawingnumber prefix to group into.
    ''' </summary>
    ''' <returns>The length.</returns>
    Public ReadOnly Property GroupPrefixLength As Integer
    ''' <summary>
    ''' The index of the selected group.
    ''' </summary>
    ''' <returns>The index.</returns>
    Public ReadOnly Property SelectedGroupIndex As Integer

    ''' <summary>
    ''' The tooltip to use on this palette.
    ''' </summary>
    Private ReadOnly _toolTip As New ToolTip With {.AutomaticDelay = 200}

    ''' <summary>
    ''' Determines if this palette is fully loaded.
    ''' </summary>
    Private _loaded As Boolean = False
    ''' <summary>
    ''' Determines if frameselection was by a single click.
    ''' <para>In that case, a reload of the framelist isn't needed.</para>
    ''' </summary>
    Private _singleClick As Boolean = False
    ''' <summary>
    ''' A timer to delay the display of the image/text balloon.
    ''' </summary>
    Private _mouseHoverTimer As Timer
    ''' <summary>
    ''' Keeps the index of the row of the framelist that the mouse hovers over.
    ''' </summary>
    Private _mouseHoverRowIndex As Integer = -1
    ''' <summary>
    ''' The present framelist database.
    ''' </summary>
    Private _frameListData As DataTable
    ''' <summary>
    ''' The framelist records divided in groups.
    ''' </summary>
    Private _groupedRows As Dictionary(Of String, List(Of DataRow))
    ''' <summary>
    ''' The name of the selected group.
    ''' </summary>
    Private _selectedGroup As String
    ''' <summary>
    ''' Contains a list of (last) letters of the drawingnumbers for a possible filter.
    ''' </summary>
    Private _filters As List(Of String)

    'form

    ''' <summary>
    ''' Initializes a new instance of <see cref="FramesPalette"/>.
    ''' <para><see cref="FramesPalette"/> provides the design and functionality for the frames tab of the <see cref="PaletteSet"/>.</para>
    ''' </summary>
    Sub New()
        ' This call is required by the designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.

        AddHandler Translator.LanguageChangedEvent, AddressOf LanguageChangedEventHandler
        FilterListBox.Visible = False
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when user clicks on an empty part of this palette.
    ''' <para>If screenshots are enabled (by doubleclicking on the languageflag, see <see cref="SettingsPalette"/>) a screenshot of this palette will be taken.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Me_Click(sender As Object, e As EventArgs) Handles Me.Click
        Try
            If ScreenShot Then ImageHelper.GetScreenShot(PaletteHelper.GetLocation, PaletteHelper.GetSize)
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when this palette is resizing.
    ''' <para>It handles the size and position of controls it contains.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Me_SizeChanged(sender As Object, e As EventArgs) Handles Me.SizeChanged
        Try
            If Me.Height = 0 Then Exit Sub
            Dim newHeight = {300, Me.Height}.Max
            Dim newWidth = {120, Me.Width}.Max
            Dim spaceBelow = 24

            Dim heightDifference = newHeight - (OpenFolderButton.Top + OpenFolderButton.Height + spaceBelow)
            If Not heightDifference = 0 Then
                FramesDataGridView.Height += heightDifference
                OpenFolderButton.Top += heightDifference
                UpButton.Top += heightDifference
                DownButton.Top += heightDifference
                GroupsListBox.Top += heightDifference
                ltSelectAll.Top += heightDifference
                GroupingLabel.Top += heightDifference
                ltThisFileOnly.Top += heightDifference
                FilterListBox.Top += heightDifference
                FilterButton.Top += heightDifference
                If _loaded Then ReloadGridView()
            End If

            Dim widthDifference = newWidth - (OpenFolderButton.Left + OpenFolderButton.Width + 8)
            If FramesDataGridView.Width + widthDifference < 25 Then widthDifference = 25 - FramesDataGridView.Width
            If Not widthDifference = 0 Then
                FramesDataGridView.Width += widthDifference
                GroupsListBox.Width += widthDifference
                FilterListBox.Width += widthDifference
                ltSelectAll.Width += widthDifference
                UpButton.Left += widthDifference
                DownButton.Left += widthDifference
                ZoomExtentsButton.Left += widthDifference
                OpenFolderButton.Left += widthDifference
                OverviewButton.Left += widthDifference
                GroupingLabel.Left += widthDifference
                FilterButton.Left += widthDifference
                Select Case GroupsListBox.SelectedIndex < GroupsListBox.Items.Count - 1
                    Case True : FramesDataGridView.SetColumnWidths("Drawing=45%;Sheet=30%;FrameSize=25%".Cut.ToIniDictionary)
                    Case Else : FramesDataGridView.SetColumnWidths("Filename=100%;Filedate=0".Cut.ToIniDictionary)
                End Select
            End If
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    'subs

    ''' <summary>
    ''' Reloads the framelist and enables/disables controls as needed.
    ''' </summary>
    Sub ReloadGridView()
        If _singleClick Then _singleClick = False : Exit Sub
        SetFilter(False)
        _loaded = True
        Dim doc = ActiveDocument()
        If IsNothing(doc) Then ClearPaletteWhenNoActiveDocument() : Exit Sub

        Select Case True
            Case doc.IsNamedDrawing
                _toolTip.SetToolTip(OpenFolderButton, doc.GetPath)
                Dim frameSetController = New FrameSetHandler(doc.Name, False)
                _frameListData = frameSetController.UpdatedFrameListData
                _GroupPrefixLength = {If(PrefixLengths.ContainsKey(doc.GetPath), PrefixLengths(doc.GetPath), 0), 0}.Max
            Case Else
                _frameListData = FrameHelper.ConvertToFramelist(doc.FrameData)
                _GroupPrefixLength = -1
        End Select

        Dim maxRows = CInt((FramesDataGridView.Height - 26) / 15) 'rowheight=15, header+footer=26
        Dim groups = GetGroups(maxRows)
        Dim items = New List(Of String)
        Dim selectedIndex = -1
        For i = 0 To groups.Count - 1
            items.Add("{0}   ({1})".Compose(groups(i), _groupedRows(groups(i)).Count))
            If groups(i) = _selectedGroup Then selectedIndex = i
        Next
        If Not ltThisFileOnly.Checked And doc.IsNamedDrawing Then
            items.Add("Frameless".Translate(_frameListData.GetTable("Files").Rows.Count))
        End If
        GroupsListBox.Items.Clear()
        GroupsListBox.Items.AddRange(items.ToArray)

        Select Case True
            Case _selectedGroup = "Frameless" : GroupsListBox.SetSelected(GroupsListBox.Items.Count - 1, True)
            Case doc.NotNamedDrawing : GroupsListBox.SetSelected(0, True)
            Case selectedIndex = -1
                If GroupsListBox.Items.Count > 0 Then
                    GroupsListBox.SetSelected(0, True)
                Else
                    ShowSelectedGroup()
                End If
            Case selectedIndex < GroupsListBox.Items.Count : GroupsListBox.SetSelected(selectedIndex, True)
            Case GroupsListBox.Items.Count = 0
            Case Else : GroupsListBox.SetSelected(0, True)
        End Select

        OverviewButton.Enabled = doc.IsNamedDrawing
        ltThisFileOnly.Enabled = doc.IsNamedDrawing
        OpenFolderButton.Enabled = doc.IsNamedDrawing
        UpButton.Enabled = doc.IsNamedDrawing And Not ltThisFileOnly.Checked
        DownButton.Enabled = doc.IsNamedDrawing And Not ltThisFileOnly.Checked
        ZoomExtentsButton.Enabled = True
        ltFrames.Enabled = True
        ltSelectAll.Enabled = True

        GroupingLabel.Text = {_GroupPrefixLength, 0}.Max
        Select Case PrefixLengths.ContainsKey(doc.GetPath) AndAlso PrefixLengths(doc.GetPath) > -1
            Case True : GroupingLabel.Font = FontHelper.SansSerifBold
            Case Else : GroupingLabel.Font = FontHelper.SansSerifRegular
        End Select
        GroupingLabel.Refresh()
    End Sub

    ''' <summary>
    ''' Reloads the contextmenu of the framelist as needed.
    ''' </summary>
    Sub ReloadMenuStrip()
        Dim doc = ActiveDocument()
        Dim menuData = DataSetHelper.LoadFromXml("{Support}\SetContextMenus.xml".Compose)
        Dim contextMenuStrip = New ContextMenuStrip
        Select Case True
            Case FramesDataGridView.Rows.Count = 0
                contextMenuStrip = MenuStripHelper.Create(menuData.GetTable("Framesless"), AddressOf ContextMenuStripClickEventHandler, My.Resources.ResourceManager)
                FramesDataGridView.ContextMenuStrip = contextMenuStrip
                ltSelectAll.ContextMenuStrip = contextMenuStrip
            Case doc.NotNamedDrawing Or ltThisFileOnly.Checked
                contextMenuStrip = MenuStripHelper.Create(menuData.GetTable("WithFrames"), AddressOf ContextMenuStripClickEventHandler, My.Resources.ResourceManager)
                FramesDataGridView.ContextMenuStrip = contextMenuStrip
                ltSelectAll.ContextMenuStrip = contextMenuStrip
            Case GroupsListBox.SelectedIndex < GroupsListBox.Items.Count - 1
                contextMenuStrip = MenuStripHelper.Create(menuData.GetTable("WithFrames"), AddressOf ContextMenuStripClickEventHandler, My.Resources.ResourceManager)
                FramesDataGridView.ContextMenuStrip = contextMenuStrip
                ltSelectAll.ContextMenuStrip = contextMenuStrip
            Case Else
                contextMenuStrip = MenuStripHelper.Create(menuData.GetTable("Framesless"), AddressOf ContextMenuStripClickEventHandler, My.Resources.ResourceManager)
                FramesDataGridView.ContextMenuStrip = contextMenuStrip
                ltSelectAll.ContextMenuStrip = contextMenuStrip
        End Select
        AddHandler contextMenuStrip.VisibleChanged, AddressOf ContextMenuStripVisibleChangedEventHandler
        contextMenuStrip.Hide()
    End Sub

    'buttons

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
    ''' EventHandler for the event that occurs when the user clicks the ZoomExtents button.
    ''' <para>It selects the model-layout-tab and zooms to the extents of it.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ZoomExtentsButton_Click(sender As Object, e As EventArgs) Handles ZoomExtentsButton.Click
        Try
            Dim doc = ActiveDocument()
            LayoutHelper.SelectModel(doc)
            doc.ZoomExtents
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks the Overview button.
    ''' <para>It shows the <see cref="OverviewDialog"/>.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub OverviewButton_Click(sender As Object, e As EventArgs) Handles OverviewButton.Click
        Try
            Dim dialog = New OverviewDialog(False)
            ReloadGridView()
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks the OpenFolder button.
    ''' <para>It opens the folder of the present document in explorer.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub OpenFolderButton_Click(sender As Object, e As EventArgs) Handles OpenFolderButton.Click
        Try
            Dim doc = ActiveDocument()
            If doc.IsNamedDrawing Then ProcessHelper.StartExplorer(doc.GetPath)
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks the Up button.
    ''' <para>It increases the length of the groupprefix.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub UpButton_Click(sender As Object, e As EventArgs) Handles UpButton.Click
        Try
            Dim doc = ActiveDocument()
            If doc.IsNamedDrawing Then
                _GroupPrefixLength = {_GroupPrefixLength + 1, 10}.Min
                Select Case PrefixLengths.ContainsKey(doc.GetPath)
                    Case True : PrefixLengths(doc.GetPath) = _GroupPrefixLength
                    Case Else : PrefixLengths.Add(doc.GetPath, _GroupPrefixLength)
                End Select
                ReloadGridView()
            End If
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks the Down button.
    ''' <para>It shortens the length of the group prefix.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub DownButton_Click(sender As Object, e As EventArgs) Handles DownButton.Click
        Try
            Dim doc = ActiveDocument()
            If doc.IsNamedDrawing Then
                _GroupPrefixLength = {_GroupPrefixLength - 1, -1}.Max
                Select Case PrefixLengths.ContainsKey(doc.GetPath)
                    Case True : PrefixLengths(doc.GetPath) = _GroupPrefixLength
                    Case Else : PrefixLengths.Add(doc.GetPath, _GroupPrefixLength)
                End Select
                ReloadGridView()
            End If
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks the Filter button.
    ''' <para>It toggeles the filter mode.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub FilterButton_Click(sender As Object, e As EventArgs) Handles FilterButton.Click
        Try
            SetFilter(Not FilterListBox.Visible)
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    'checkboxes

    ''' <summary>
    ''' EventHandler for the event that occurs when the user toggles the ThisFileOnly checkbox.
    ''' <para>It reloads the framelist.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ThisFileOnlyCheckBox_CheckedChanged(sender As Object, e As EventArgs) Handles ltThisFileOnly.CheckedChanged
        Try
            ReloadGridView()
        Catch ex As Exception
            GadecException(ex)
        End Try
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
            ShowSelectedGroup()
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user changes the filter selection.
    ''' <para>It loads the framelist filtered with a new filter.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub FilterListBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles FilterListBox.SelectedIndexChanged
        Try
            If Not FilterListBox.Visible Then Exit Sub

            Dim filterString = String.Join("", _filters.ToArray)
            Dim selectedItems = FilterListBox.SelectedItems
            If selectedItems.Count > 0 Then
                filterString = ""
                For Each selectedItem In selectedItems
                    filterString &= selectedItem
                Next
            End If
            Dim currencyManager = CType(FramesDataGridView.BindingContext(FramesDataGridView.DataSource), CurrencyManager)
            currencyManager.SuspendBinding()
            Try
                Dim count = 0
                For Each row In FramesDataGridView.Rows.ToArray
                    Dim patternResult = row.Cells("Drawing").Value.ToString.FindResultAsPatternRev("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ")
                    Select Case True
                        Case patternResult.Result = "" : row.Visible = False
                        Case filterString.Contains(patternResult.Result.RightString(1)) : row.Visible = True : count += 1
                        Case Else : row.Visible = False
                    End Select
                Next
                ltFrames.Text = "ltFrames".Translate(count)
            Catch ex As Exception
                ex.AddData($"Filter: {filterString}")
                ex.Rethrow
            Finally
                currencyManager.ResumeBinding()
            End Try
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    'datagridviews

    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks in the framelist (not on a cell).
    ''' <para>It zooms to the extents of the drawing.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub FramesDataGridView_Click(sender As Object, e As EventArgs) Handles FramesDataGridView.Click
        Try
            If IsNothing(ActiveDocument) Or FramesDataGridView.Rows.Count > 0 Then Exit Sub

            With ActiveDocument()
                .CancelCommand
                .ZoomExtents
            End With
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks in a cell of the framelist to select it.
    ''' <para>It selects the tab of the document and sets its view to the selected frame, if that document is already open.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub FramesDataGridView_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles FramesDataGridView.CellClick
        Try
            If e.RowIndex < 0 Or Not FramesDataGridView.SelectedRows.Count = 1 Then Exit Sub
            Dim doc = ActiveDocument()
            Dim documents = DocumentsHelper.GetDocumentNames()
            Dim fileName = "{0}\{1}".Compose(doc.GetPath, FramesDataGridView.Item("Filename", e.RowIndex).Value)
            Dim num = ""
            Select Case True
                Case Not FramesDataGridView.Columns.Contains("Num")
                    If documents.Contains(fileName) Then DocumentsHelper.Open(fileName)
                Case doc.NotNamedDrawing
                    fileName = FramesDataGridView.Item("Filename", e.RowIndex).Value
                    num = FramesDataGridView.Item("Num", e.RowIndex).Value
                    doc.ActiveFrame(num)
                    Dim frameIdCollection = XRecordObjectIdsHelper.Load(doc.Database, "{Company}".Compose, "FrameWorkIDs", num)
                    FrameViewHelper.SetView(doc, frameIdCollection(0))
                Case doc.Name = fileName
                    num = FramesDataGridView.Item("Num", e.RowIndex).Value
                    doc.ActiveFrame(num)
                    Dim frameIdCollection = XRecordObjectIdsHelper.Load(doc.Database, "{Company}".Compose, "FrameWorkIDs", num)
                    FrameViewHelper.SetView(doc, frameIdCollection(0))
                Case documents.Contains(fileName)
                    _singleClick = True
                    Dim openDoc = DocumentsHelper.Open(fileName)
                    If NotNothing(openDoc) Then
                        num = FramesDataGridView.Item("Num", e.RowIndex).Value
                        openDoc.ActiveFrame(num)
                        Dim frameIdCollection = XRecordObjectIdsHelper.Load(openDoc.Database, "{Company}".Compose, "FrameWorkIDs", num)
                        If frameIdCollection.Count > 0 Then FrameViewHelper.SetView(openDoc, frameIdCollection(0))
                    End If
            End Select
            FramesDataGridView.Focus()
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user doubleclicks a cell of the framelist to select it.
    ''' <para>The document containing the frame will be opened and sets its view to the selected frame.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub FramesDataGridView_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs) Handles FramesDataGridView.CellDoubleClick
        Try
            If e.RowIndex < 0 Or Not FramesDataGridView.SelectedRows.Count = 1 Then Exit Sub
            Dim doc = ActiveDocument()
            Dim file = ""
            Dim num = ""
            Select Case doc.IsNamedDrawing
                Case True : file = "{0}\{1}".Compose(doc.GetPath, FramesDataGridView.Item("Filename", e.RowIndex).Value)
                Case Else : file = FramesDataGridView.Item("Filename", e.RowIndex).Value
            End Select
            Select Case FramesDataGridView.Columns.Contains("Num")
                Case True : num = FramesDataGridView.Item("Num", e.RowIndex).Value
                Case Else : num = ""
            End Select
            _singleClick = False
            Dim openDoc = DocumentsHelper.Open(file)
            Select Case True
                Case IsNothing(openDoc)
                Case num = ""
                    openDoc.ActiveFrame(num)
                Case Else
                    openDoc.ActiveFrame(num)
                    Dim frameIdCollection = XRecordObjectIdsHelper.Load(openDoc.Database, "{Company}".Compose, "FrameWorkIDs", num)
                    If frameIdCollection.Count > 0 Then FrameViewHelper.SetView(openDoc, frameIdCollection(0))
                    ReloadGridView()
            End Select
            FramesDataGridView.Focus()
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user rightclicks in a cell of the framelist.
    ''' <para>If the row is not already selected, the selection is cleared and that row selected.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub FramesDataGridView_CellMouseDown(sender As Object, e As DataGridViewCellMouseEventArgs) Handles FramesDataGridView.CellMouseDown
        Try
            Select Case True
                Case Not e.Button = MouseButtons.Right
                Case e.RowIndex < 0
                Case FramesDataGridView.Rows(e.RowIndex).Selected
                Case Else
                    FramesDataGridView.ClearSelection()
                    FramesDataGridView.Rows(e.RowIndex).Selected = True
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
            If NotNothing(_mouseHoverTimer) Then
                _mouseHoverTimer.Dispose()
                _mouseHoverTimer = Nothing
            End If
            If e.RowIndex < 0 OrElse Not PaletteHelper.FramePageHasFocus Then Exit Sub

            FramesDataGridView.Rows(e.RowIndex).HighlightRow(True)
            If Registerizer.UserSetting("HidePreviews") = "True" Then Exit Sub

            _mouseHoverRowIndex = e.RowIndex
            _mouseHoverTimer = New Timer
            AddHandler _mouseHoverTimer.Tick, AddressOf TimerTickEventHandler
            _mouseHoverTimer.Interval = 350
            _mouseHoverTimer.Enabled = True
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
            If NotNothing(_mouseHoverTimer) Then
                _mouseHoverTimer.Enabled = False
            End If
            If e.RowIndex < 0 OrElse Not PaletteHelper.FramePageHasFocus Then Exit Sub

            FramesDataGridView.Rows(e.RowIndex).HighlightRow(False)
            If Registerizer.UserSetting("HidePreviews") = "True" Then Exit Sub

            If FramesDataGridView.Columns.Contains("Num") Then
                BalloonHelper.ShowFramePreview(Nothing, Nothing, Nothing)
            ElseIf FramesDataGridView.Columns.Contains("Filename") Then
                BalloonHelper.ShowDocumentPreview(Nothing, Nothing, Nothing)
            End If
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    'eventHandlers

    ''' <summary>
    ''' EventHandler for the event that occurs when the language has changed.
    ''' <para>It translates this palette.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub LanguageChangedEventHandler(sender As Object, e As LanguageChangedEventArgs)
        _toolTip.SetToolTip(OpenFolderButton, "")
        Translator.TranslateControls(Me)

        _toolTip.SetToolTip(OverviewButton, "cDrawinglist".Translate)
        _toolTip.SetToolTip(ZoomExtentsButton, "cZoomExtents".Translate)
        _toolTip.SetToolTip(FilterButton, "tFilterButton on".Translate)
        _toolTip.SetToolTip(UpButton, "tUp".Translate)
        _toolTip.SetToolTip(DownButton, "tDown".Translate)

        _toolTip.SetToolTip(FilterListBox, "tFilter".Translate)
        _toolTip.SetToolTip(GroupsListBox, "tDiv".Translate)

        ReloadGridView()
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the timer reaches the specified time.
    ''' <para>It displays the image/text balloon.</para>
    ''' </summary>
    Private Sub TimerTickEventHandler()
        Try
            _mouseHoverTimer.Enabled = False
            If _mouseHoverRowIndex = -1 Or Not _mouseHoverRowIndex < FramesDataGridView.Rows.Count Then Exit Sub

            Dim dataRow = FramesDataGridView.Rows(_mouseHoverRowIndex)
            Dim rowRectangle = FramesDataGridView.GetRowDisplayRectangle(_mouseHoverRowIndex, True)
            Dim pointX = 0
            Dim pointY = rowRectangle.Y + FramesDataGridView.Top + (rowRectangle.Height / 2)
            Dim alignment As HorizontalAlignment = PaletteHelper.GetTitlebarLocation
            Select Case alignment = HorizontalAlignment.Left
                Case True : pointX = rowRectangle.X + FramesDataGridView.Left + FramesDataGridView.Width + 10
                Case Else : pointX = rowRectangle.X + FramesDataGridView.Left - 10
            End Select
            Dim displayPoint = PointToScreen(New Drawing.Point(pointX, pointY))
            Select Case True
                Case FramesDataGridView.Columns.Contains("Num") : BalloonHelper.ShowFramePreview(dataRow, displayPoint, alignment)
                Case FramesDataGridView.Columns.Contains("Filename") : BalloonHelper.ShowDocumentPreview(dataRow, displayPoint, alignment)
            End Select
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
            Dim documents = DocumentsHelper.GetDocumentNames()
            Dim doc = ActiveDocument()
            doc.CancelCommand
            Dim tag = DirectCast(sender, ToolStripItem).Tag.ToString
            Dim frameSelection = New Dictionary(Of String, String)
            For Each row In FramesDataGridView.Rows.ToArray
                If row.Selected And row.Visible Then
                    Dim file = row.Cells("Filename").Value.ToString
                    If doc.IsNamedDrawing Then file = "{0}\{1}".Compose(doc.GetPath, file)
                    Select Case True
                        Case Not FramesDataGridView.Columns.Contains("Num") : frameSelection.TryAdd(file, "$$$")
                        Case frameSelection.ContainsKey(file) : frameSelection(file) &= ";{0}".Compose(row.Cells("Num").Value)
                        Case Else : frameSelection.Add(file, row.Cells("Num").Value)
                    End Select
                End If
            Next
            If FramesDataGridView.Rows.Count = 0 Then frameSelection.TryAdd(doc.Name, "$$$")
            If frameSelection.Count = 0 Then Exit Sub

            Try
                Select Case tag

                    '//Frames options:
                    Case "ToA3", "ToA4"
                        Dim copies = GetNumberOfCopies()
                        If copies < 1 Then Exit Sub

                        DocumentEvents.DocumentEventsEnabled = False
                        Dim framePlotter = New FramePlotter(frameSelection, tag)
                        framePlotter.PlotMultiPage(copies)
                    Case "ToPLT"
                        Dim copies = GetNumberOfCopies()
                        If copies < 1 Then Exit Sub

                        DocumentEvents.DocumentEventsEnabled = False
                        Dim framePlotter = New FramePlotter(frameSelection, tag)
                        framePlotter.PlotMultiFrame(copies)
                    Case "ToPDF"
                        Dim projectName = If(frameSelection.Count > 1, doc.GetPath.InStrRevResult("\"), IO.Path.GetFileNameWithoutExtension(frameSelection.First.Key))
                        Dim initialFile = If(doc.IsNamedDrawing, "{0}\{1}.pdf", "\{1}.pdf").Compose(doc.GetPath, projectName)
                        Dim fileName = FileSystemHelper.FileSaveAs(initialFile)
                        If fileName = "" Then Exit Sub

                        DocumentEvents.DocumentEventsEnabled = False
                        FileSystemHelper.DeleteAllFiles("{TempFolder}".Compose)
                        Dim files = New List(Of String)
                        Using sysVar = New SysVarHandler(doc)
                            sysVar.Set("PDFSHX", 0)
                            Dim framePlotter = New FramePlotter(frameSelection, tag)
                            files.AddRange(framePlotter.PlotMultiFrame(1))
                        End Using
                        If files.Count = 0 Then Exit Sub

                        Using pdfDocument = PdfSharpHelper.MergePdfFiles(files.ToArray)
                            pdfDocument.Save(fileName)
                        End Using
                        If Registerizer.UserSetting("OpenPdf") = "True" Then
                            Dim start = New ProcessWithEvents(fileName)
                        End If
                    Case "ToPDFset"
                        Using drawingList = New DrawinglistCreator(frameSelection, _frameListData, _GroupPrefixLength)
                            If drawingList.NoSelection Then Exit Sub

                            Dim dialog = New CoverSheetDialog(drawingList.GetDrawinglistHeader, True, doc.GetPath)
                            If Not dialog.DialogResult = DialogResult.OK Then Exit Sub

                            drawingList.Create(True, dialog.Attachments, dialog.OnlyAttachments)
                            If drawingList.CreateFailure Then Exit Sub

                            Dim initialFile = If(doc.IsNamedDrawing, "{0}\{1}.pdf", "\{1}.pdf").Compose(doc.GetPath, drawingList.ProjectName)
                            Dim fileName = FileSystemHelper.FileSaveAs(initialFile)
                            If fileName = "" Then Exit Sub

                            DocumentEvents.DocumentEventsEnabled = False
                            FileSystemHelper.DeleteAllFiles("{TempFolder}".Compose)
                            Dim files = New List(Of String)
                            Dim framePlotter = New FramePlotter(drawingList.Selection, "ToPDF")
                            Using sysVar = New SysVarHandler(doc)
                                sysVar.Set("PDFSHX", 0)
                                Using layerVisiblizer = New LayerVisiblizer(doc)
                                    files.AddRange(framePlotter.PlotExternalDrawing(drawingList.GetDatabase))
                                End Using
                                If Not dialog.OnlyAttachments Then files.AddRange(framePlotter.PlotMultiFrame(1))
                            End Using
                            If files.Count = 0 Then Exit Sub

                            Using pdfDocument = PdfSharpHelper.MergePdfFiles(files.ToArray)
                                If dialog.Attachments.Count > 0 Then PdfSharpHelper.AddPdfFiles(pdfDocument, dialog.Attachments)
                                Dim rects = {New System.Windows.Rect(120.0, 195.0, 37.4, 13.75), New System.Windows.Rect(157.5, 195.0, 37.4, 13.75)}
                                PdfSharpHelper.AddSignFields(pdfDocument, 0, rects, drawingList.ProjectName)
                                pdfDocument.Save(fileName)
                                pdfDocument.Close()
                            End Using
                            Dim start = New ProcessWithEvents(fileName)
                        End Using
                    Case "DwgList"
                        DocumentEvents.DocumentEventsEnabled = True
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
                    Case "SetLayout"
                        DocumentEvents.DocumentEventsEnabled = False
                        Dim framePlotter = New FramePlotter(frameSelection, "ToPDF")
                        framePlotter.SetPlotLayout()
                    Case "EditH"
                        If doc.IsNamedDrawing Then
                            Dim frameSetController = New FrameSetHandler(doc.Name, False)
                            _frameListData = frameSetController.UpdatedFrameListData
                        End If
                        DocumentEvents.DocumentEventsEnabled = doc.NotNamedDrawing
                        FrameHeaderHelper.EditHeader(doc, frameSelection, _frameListData)
                    Case "AddRev"
                        DocumentEvents.DocumentEventsEnabled = doc.NotNamedDrawing
                        FrameHeaderHelper.AddRevision(doc, frameSelection)
                    Case "UpdateStatus"
                        DocumentEvents.DocumentEventsEnabled = False
                        FrameStampHelper.UpdateStatus(doc, frameSelection, _frameListData)
                    Case "Preview"
                        DocumentEvents.DocumentEventsEnabled = False
                        Dim framePlotter = New FramePlotter(frameSelection, "ToPDF")
                        Dim plotStatus = framePlotter.PlotPreview()
                        If Not plotStatus = Autodesk.AutoCAD.PlottingServices.PreviewEndPlotStatus.Plot Then Exit Sub

                        Dim copies = GetNumberOfCopies()
                        If copies < 1 Then Exit Sub

                        framePlotter = New FramePlotter(frameSelection, "ToPLT")
                        framePlotter.PlotMultiFrame(copies)

                        '//frameless options:
                    Case "NormalToPLT"
                        DocumentEvents.DocumentEventsEnabled = False
                        Dim framePlotter = New FramePlotter(frameSelection, tag)
                        framePlotter.PlotFramelessDrawings()
                    Case "NormalToPDF"
                        Dim projectName = If(frameSelection.Count > 1, doc.GetPath.InStrRevResult("\"), IO.Path.GetFileNameWithoutExtension(frameSelection.First.Key))
                        Dim initialFile = If(doc.IsNamedDrawing, "{0}\{1}.pdf", "\{1}.pdf").Compose(doc.GetPath, projectName)
                        Dim fileName = FileSystemHelper.FileSaveAs(initialFile)
                        If fileName = "" Then Exit Sub

                        DocumentEvents.DocumentEventsEnabled = False
                        FileSystemHelper.DeleteAllFiles("{TempFolder}".Compose)
                        Dim files = New List(Of String)
                        Using sysVar = New SysVarHandler(doc)
                            sysVar.Set("PDFSHX", 0)
                            Dim framePlotter = New FramePlotter(frameSelection, tag)
                            files.AddRange(framePlotter.PlotFramelessDrawings)
                        End Using
                        If files.Count = 0 Then Exit Sub

                        Using pdfDocument = PdfSharpHelper.MergePdfFiles(files.ToArray)
                            If pdfDocument.PageCount > 0 Then pdfDocument.Save(fileName)
                        End Using
                        If Registerizer.UserSetting("OpenPdf") = "True" Then
                            Dim start = New ProcessWithEvents(fileName)
                        End If
                    Case "EditH_NF"
                        DocumentEvents.DocumentEventsEnabled = False
                        Dim files = frameSelection.Keys.ToList
                        For Each file In files
                            Dim editHeaderDoc = DocumentsHelper.Open(file)
                            If NotNothing(editHeaderDoc) Then FrameHeaderHelper.EditFramelessHeader(editHeaderDoc)
                            If editHeaderDoc.WasClosed Then DocumentsHelper.Close({editHeaderDoc}, True)
                        Next
                    Case "AddRev_NF"
                        DocumentEvents.DocumentEventsEnabled = False
                        Dim files = frameSelection.Keys.ToList
                        For Each file In files
                            Dim addRevisionDoc = DocumentsHelper.Open(file)
                            If NotNothing(addRevisionDoc) Then FrameHeaderHelper.AddFramelessRevision(addRevisionDoc)
                            If addRevisionDoc.WasClosed Then DocumentsHelper.Close({addRevisionDoc}, True)
                        Next
                    Case "FrameSearch"
                        DocumentEvents.DocumentEventsEnabled = False
                        FrameFindHelper.SearchThroughDocuments(frameSelection.Keys.ToArray)
                        '//Note: The examined files are read in again because the date of the files has changed.

                End Select
            Catch ex As Exception
                If Not ex.Message = "ePageCancelled" Then ex.Rethrow
            Finally
                If Not DocumentEvents.DocumentEventsEnabled Then
                    DocumentsHelper.Open(doc.Name)
                    DocumentEvents.DocumentEventsEnabled = True
                    ReloadGridView()
                End If
            End Try
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
    ''' Shows the selected group of frames.
    ''' </summary>
    Private Sub ShowSelectedGroup()
        Dim doc = ActiveDocument()
        Select Case True
            Case ltThisFileOnly.Checked And GroupsListBox.SelectedIndex = -1
                FramesDataGridView.Columns.Clear()
                FramesDataGridView.DataSource = Nothing
                FilterButton.Enabled = False
            Case ltThisFileOnly.Checked Or doc.NotNamedDrawing Or GroupsListBox.SelectedIndex < GroupsListBox.Items.Count - 1
                _SelectedGroupIndex = GroupsListBox.SelectedIndex
                Dim rows = _groupedRows(GroupsListBox.SelectedItem.ToString.Cut("   ").Item(0))
                FramesDataGridView.Columns.Clear()
                FramesDataGridView.DataSource = Nothing
                If rows.Count > 0 Then
                    Dim frameListData = rows.CopyToDataTable
                    frameListData.AssignDefaultViewSort("Drawing;Sheet")
                    FramesDataGridView.DataSource = frameListData
                End If
                FramesDataGridView.SetColumnWidths("Drawing=45%;Sheet=30%;FrameSize=25%".Cut.ToIniDictionary)
                ltFrames.Text = "ltFrames".Translate(FramesDataGridView.Rows.Count)
                'filter
                _filters = GetFilters(rows).ToSortedList
                FilterListBox.Items.Clear()
                FilterListBox.Items.AddRange(_filters.ToArray)
                SetFilter(False)
                FilterButton.Enabled = True
            Case Else
                FramesDataGridView.Columns.Clear()
                FramesDataGridView.DataSource = Nothing
                Dim fileListData = _frameListData.GetTable("Files")
                fileListData.AssignDefaultViewSort("Filename")
                FramesDataGridView.DataSource = fileListData
                FramesDataGridView.SetColumnWidths("Filename=100%;Filedate=0".Cut.ToIniDictionary)
                ltFrames.Text = "ltDrawings".Translate(FramesDataGridView.Rows.Count)
                FilterButton.Enabled = False
        End Select
        FramesDataGridView.AllowUserToOrderColumns = False
        FramesDataGridView.Focus()
        FramesDataGridView.ClearSelection()
        Dim documents = DocumentsHelper.GetDocumentNames()
        For Each row In FramesDataGridView.Rows.ToArray
            Dim file = row.Cells("Filename").Value
            If doc.IsNamedDrawing Then file = "{0}\{1}".Compose(doc.GetPath, file)
            If documents.Contains(file) Then row.DefaultCellStyle.BackColor = Drawing.Color.PeachPuff
            Select Case True
                Case Not file = doc.Name
                Case Not FramesDataGridView.Columns.Contains("Num") : SelectRow(row)
                Case doc.ActiveFrame = "" : doc.ActiveFrame(row.Cells("Num").Value) : SelectRow(row)
                Case doc.ActiveFrame = row.Cells("Num").Value : SelectRow(row)
            End Select
        Next
        ReloadMenuStrip()
    End Sub

    Private Sub SelectRow(row As DataGridViewRow)
        row.Selected = True
        For Each cell In row.Cells.ToArray
            If cell.Visible Then row.DataGridView.CurrentCell = cell : Exit For
        Next
    End Sub

    ''' <summary>
    ''' Gets the available filter letters.
    ''' </summary>
    ''' <param name="rows"></param>
    ''' <returns>A list of filter letters.</returns>
    Private Function GetFilters(rows As List(Of DataRow)) As List(Of String)
        Dim output = New List(Of String)
        For Each row In rows
            Dim patternResult = row.GetString("Drawing").FindResultAsPatternRev("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ")
            Dim filterType = patternResult.Result.RightString(1)
            If output.Contains(filterType) OrElse filterType = "" Then Continue For

            output.Add(filterType)
        Next
        Return output
    End Function

    ''' <summary>
    ''' Sets the filter mode.
    ''' </summary>
    ''' <param name="value"></param>
    Private Sub SetFilter(value As Boolean)
        FilterListBox.ClearSelected()
        Select Case value
            Case True
                GroupsListBox.Visible = False
                FilterListBox.Visible = True
                FilterButton.BackColor = Drawing.Color.LightSkyBlue
                _toolTip.SetToolTip(FilterButton, "tFilterButton off".Translate)
            Case Else
                GroupsListBox.Visible = True
                FilterListBox.Visible = False
                FilterButton.UseVisualStyleBackColor = True
                _toolTip.SetToolTip(FilterButton, "tFilterButton on".Translate)
        End Select
    End Sub

    ''' <summary>
    ''' Clears this palette when no document is active (eg. startscreen of AutoCAD).
    ''' </summary>
    Private Sub ClearPaletteWhenNoActiveDocument()
        FramesDataGridView.Columns.Clear()
        FramesDataGridView.ContextMenuStrip = Nothing
        ltSelectAll.ContextMenuStrip = Nothing
        GroupsListBox.Items.Clear()
        ltFrames.Text = "Start"
        GroupingLabel.Text = "0"
        GroupingLabel.Font = FontHelper.SansSerifRegular
        OverviewButton.Enabled = False
        ZoomExtentsButton.Enabled = False
        ltSelectAll.Enabled = False
        UpButton.Enabled = False
        DownButton.Enabled = False
        OpenFolderButton.Enabled = False
        ltFrames.Enabled = False
        ltThisFileOnly.Enabled = False
        FilterButton.Enabled = False
    End Sub

    'private functions

    ''' <summary>
    ''' Gets the groupslist of the framelist.
    ''' <para>Note: If there are many frames, it will be grouped automatically.</para>
    ''' </summary>
    ''' <param name="maxRows"></param>
    ''' <returns>A list of groupstrings.</returns>
    Private Function GetGroups(maxRows As Integer) As String()
        Dim doc = ActiveDocument()
        PrefixLengths.TryAdd(doc.GetPath, -1)
        _selectedGroup = ""
        Select Case True
            Case doc.NotNamedDrawing
                Dim rows = _frameListData.Select
                _groupedRows = New Dictionary(Of String, List(Of DataRow)) From {{"*", rows.ToList}}
                Return {"*"}
            Case ltThisFileOnly.Checked
                _GroupPrefixLength = -1
                Dim rows = (From row In _frameListData.Select Where row("Filename") = doc.GetFileName Select row)
                _groupedRows = New Dictionary(Of String, List(Of DataRow)) From {{"*", rows.ToList}}
                Return {"*"}
            Case Else
                Do
                    _groupedRows = New Dictionary(Of String, List(Of DataRow))
                    _selectedGroup = "Frameless"

                    For Each row In _frameListData.Select
                        Dim group = "{0}*".Compose(row.GetString("Drawing").LeftString(_GroupPrefixLength))
                        Select Case _groupedRows.ContainsKey(group)
                            Case True : _groupedRows(group).Add(row)
                            Case Else : _groupedRows.Add(group, {row}.ToList)
                        End Select
                        Select Case True
                            Case row.GetString("Num") = doc.ActiveFrame : _selectedGroup = group
                            Case Not _selectedGroup = "Frameless"
                            Case doc.Name = "{0}\{1}".Compose(doc.GetPath, row.GetString("Filename")) : _selectedGroup = group
                        End Select
                    Next
                    Dim biggestGroup = 0
                    For Each value In _groupedRows.Values
                        If biggestGroup < value.Count Then biggestGroup = value.Count
                    Next
                    If PrefixLengths(doc.GetPath) > -1 OrElse _GroupPrefixLength > 7 OrElse biggestGroup < maxRows Then Exit Do

                    _GroupPrefixLength += 1
                Loop
                Return _groupedRows.Keys.ToSortedList.ToArray
        End Select
    End Function

    ''' <summary>
    ''' Shows a listbox to select the number of copies to plot.
    ''' </summary>
    ''' <returns>The number.</returns>
    Private Function GetNumberOfCopies() As Integer
        Dim copies = 0
        Dim items = "1;2;3;4;5;6;7;8".Cut
        Dim dialog = New ListBoxDialog("SelectCopies".Translate, items)
        If dialog.DialogResult = DialogResult.OK Then copies = items(dialog.GetSelectedIndex).ToInteger
        Return copies
    End Function

End Class