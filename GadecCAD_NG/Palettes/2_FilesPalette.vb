'Gadec Engineerings Software (c) 2022
Imports System.Windows.Forms
Imports Autodesk.AutoCAD.Windows
Imports GadecCAD_NG.Extensions

''' <summary>
''' <para><see cref="FilesPalette"/> provides the design and functionality for the files tab of the <see cref="PaletteSet"/>.</para>
''' </summary>
Public Class FilesPalette
    ''' <summary>
    ''' The tooltip to use on this palette.
    ''' </summary>
    Private ReadOnly _toolTip As New ToolTip With {.AutomaticDelay = 200}

    ''' <summary>
    ''' Determines if this palette is fully loaded.
    ''' </summary>
    Private _Loaded As Boolean = False
    ''' <summary>
    ''' Determines if fileselection was by a single click.
    ''' <para>In that case, a reload of the filelist isn't needed.</para>
    ''' </summary>
    Private _singleClick As Boolean = False
    ''' <summary>
    ''' A timer to delay the display of the document preview balloon.
    ''' </summary>
    Private _mouseHoverTimer As Timer
    ''' <summary>
    ''' Keeps the index of the row of the filelist that the mouse hovers over.
    ''' </summary>
    Private _mouseHoverRowIndex As Integer = -1

    'form

    ''' <summary>
    ''' Initializes a new instance of <see cref="FilesPalette"/>.
    ''' <para><see cref="FilesPalette"/> provides the design and functionality for the files tab of the <see cref="PaletteSet"/>.</para>
    ''' </summary>
    Sub New()
        ' This call is required by the designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.

        AddHandler Translator.LanguageChangedEvent, AddressOf LanguageChangedEventHandler
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
                FilesDataGridView.Height += heightDifference
                ltSelectAll.Top += heightDifference
                OpenFolderButton.Top += heightDifference
            End If

            Dim widthDifference = newWidth - (OpenFolderButton.Left + OpenFolderButton.Width + 8)
            If Not widthDifference = 0 Then
                FilesDataGridView.Width += widthDifference
                ltSelectAll.Width += widthDifference
                OpenFolderButton.Left += widthDifference
                If FilesDataGridView.Columns.Count > 1 Then FilesDataGridView.Columns(1).Width = FilesDataGridView.Width
            End If
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    'subs

    ''' <summary>
    ''' Reloads the filelist and enables/disables controls as needed.
    ''' </summary>
    Sub ReloadGridView()
        If _singleClick Then _singleClick = False : Exit Sub
        _Loaded = True
        Dim doc = ActiveDocument()
        If IsNothing(doc) Then ClearPaletteWhenNoActiveDocument() : Exit Sub
        If doc.NotNamedDrawing Then ClearPaletteWhenNotNamedDrawing() : Exit Sub

        Dim dataBuilder = New DataBuilder("Filelist", "Fullname;Filename".Cut)
        Dim filter = "*.dwg"
        Dim filterData = DataSetHelper.LoadFromXml("{Support}\SetContextMenus.xml".Compose).GetTable("Filters", "Name")
        If NotNothing(filterData) Then
            Dim row = filterData.Rows.Find(Registerizer.UserSetting("Filters_Selected"))
            If NotNothing(row) Then
                filter = row.GetString("Data")
            End If
        End If
        ltSelectAll.Enabled = True
        OpenFolderButton.Enabled = True
        ltDrawings.Enabled = True
        _toolTip.SetToolTip(OpenFolderButton, doc.GetPath)
        Dim files = IO.Directory.GetFiles(doc.GetPath, filter).ToList
        For Each file In files
            dataBuilder.AppendValue("Fullname", file)
            dataBuilder.AppendValue("Filename", IO.Path.GetFileName(file))
            dataBuilder.AddNewlyCreatedRow()
        Next
        FilesDataGridView.Columns.Clear()
        FilesDataGridView.DataSource = Nothing
        FilesDataGridView.DataSource = dataBuilder.GetDataTable("Fullname", "Filename")
        FilesDataGridView.SetColumnWidths("Filename=100%;Filedate=0".Cut.ToIniDictionary)
        ltDrawings.Text = "ltDrawings".Translate(FilesDataGridView.Rows.Count)

        FilesDataGridView.AllowUserToOrderColumns = False
        FilesDataGridView.Focus()
        FilesDataGridView.ClearSelection()

        Dim documents = DocumentsHelper.GetDocumentNames()
        For Each row In FilesDataGridView.Rows.ToArray
            Dim file = "{0}\{1}".Compose(doc.GetPath, row.Cells("Filename").Value)
            If documents.Contains(file) Then row.DefaultCellStyle.BackColor = Drawing.Color.PeachPuff
            Select Case True
                Case Not file = doc.Name
                Case Else : row.Selected = True
            End Select
        Next
        ReloadMenuStrip(FilesDataGridView.Rows.Count)
    End Sub

    ''' <summary>
    ''' Reloads the contextmenu of the filelist as needed.
    ''' </summary>
    Sub ReloadMenuStrip(fileCount As Integer)
        Dim doc = ActiveDocument()
        Dim menuData = DataSetHelper.LoadFromXml("{Support}\SetContextMenus.xml".Compose)
        Dim contextMenuStrip = New ContextMenuStrip
        Select Case fileCount = 0
            Case True
                contextMenuStrip = MenuStripHelper.Create(menuData.GetTable("NoFiles"), AddressOf ContextMenuStripClickEventHandler, My.Resources.ResourceManager)
                FilesDataGridView.ContextMenuStrip = contextMenuStrip
                ltSelectAll.ContextMenuStrip = contextMenuStrip
            Case Else
                contextMenuStrip = MenuStripHelper.Create(menuData.GetTable("WithFiles"), AddressOf ContextMenuStripClickEventHandler, My.Resources.ResourceManager)
                FilesDataGridView.ContextMenuStrip = contextMenuStrip
                ltSelectAll.ContextMenuStrip = contextMenuStrip
        End Select
        AddHandler contextMenuStrip.VisibleChanged, AddressOf ContextMenuStripVisibleChangedEventHandler
        contextMenuStrip.Hide()
    End Sub

    'buttons

    ''' <summary>
    ''' EventHandler for the event that occurs when the user left- or rightclicks the SelectAll button.
    ''' <para>It selects all the files in the filelist.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub SelectAllButton_MouseDown(sender As Object, e As MouseEventArgs) Handles ltSelectAll.MouseDown
        Try
            If e.Button = MouseButtons.Right Or e.Button = MouseButtons.Left Then
                FilesDataGridView.SelectAll()
            End If
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
            ProcessHelper.StartExplorer(ActiveDocument.GetPath)
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    'datagridviews

    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks in a cell of the filelist to select it.
    ''' <para>It selects the tab of the document, if that document is already open.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub FilesDataGridView_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles FilesDataGridView.CellClick
        Try
            If e.RowIndex < 0 Or Not FilesDataGridView.SelectedRows.Count = 1 Then Exit Sub
            Dim doc = ActiveDocument()
            Dim documents = DocumentsHelper.GetDocumentNames()
            Dim file = "{0}\{1}".Compose(doc.GetPath, FilesDataGridView.Item("Filename", e.RowIndex).Value)
            Select Case True
                Case doc.NotNamedDrawing
                Case doc.Name = file
                Case documents.Contains(file)
                    _singleClick = True
                    DocumentsHelper.Open(file)
            End Select
            FilesDataGridView.Focus()
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user doubleclicks a cell of the filelist to select it.
    ''' <para>The document will be opened.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub FilesDataGridView_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs) Handles FilesDataGridView.CellDoubleClick
        Try
            If e.RowIndex < 0 Or Not FilesDataGridView.SelectedRows.Count = 1 Then Exit Sub
            Dim doc = ActiveDocument()
            Dim file = "{0}\{1}".Compose(doc.GetPath, FilesDataGridView.Item("Filename", e.RowIndex).Value)
            _singleClick = False
            DocumentsHelper.Open(file)
            ReloadGridView()
            FilesDataGridView.Focus()
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user rightclicks in a cell of the filelist.
    ''' <para>If the row is not already selected, the selection is cleared and that row selected.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub FilesDataGridView_CellMouseDown(sender As Object, e As DataGridViewCellMouseEventArgs) Handles FilesDataGridView.CellMouseDown
        Try
            Select Case True
                Case Not e.Button = MouseButtons.Right
                Case e.RowIndex < 0
                Case FilesDataGridView.Rows(e.RowIndex).Selected
                Case Else
                    FilesDataGridView.ClearSelection()
                    FilesDataGridView.Rows(e.RowIndex).Selected = True
            End Select
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user hovers the mouse into a cell of the filelist.
    ''' <para>It highlights the row and starts a timer that displays the document preview balloon in a delayed manner.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub FilesDataGridView_CellMouseEnter(sender As Object, e As DataGridViewCellEventArgs) Handles FilesDataGridView.CellMouseEnter
        Try
            If NotNothing(_mouseHoverTimer) Then
                _mouseHoverTimer.Dispose()
                _mouseHoverTimer = Nothing
            End If
            If e.RowIndex < 0 OrElse Not PaletteHelper.FilesPageHasFocus Then Exit Sub

            FilesDataGridView.Rows(e.RowIndex).HighlightRow(True)
            If Registerizer.UserSetting("HidePreviews") = "True" Then Exit Sub

            _mouseHoverRowIndex = e.RowIndex
            _mouseHoverTimer = New Timer
            AddHandler _mouseHoverTimer.Tick, AddressOf TimerTickEventHandler
            _mouseHoverTimer.Interval = 250
            _mouseHoverTimer.Enabled = True
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user hovers the mouse out of a cell of the filelist.
    ''' <para>It no longer highlights the row and stops the timer (for displaying the document preview balloon).</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub FilesDataGridView_CellMouseLeave(sender As Object, e As DataGridViewCellEventArgs) Handles FilesDataGridView.CellMouseLeave
        Try
            If NotNothing(_mouseHoverTimer) Then
                _mouseHoverTimer.Enabled = False
            End If
            If e.RowIndex < 0 OrElse Not PaletteHelper.FilesPageHasFocus Then Exit Sub

            FilesDataGridView.Rows(e.RowIndex).HighlightRow(False)
            If Registerizer.UserSetting("HidePreviews") = "True" Then Exit Sub

            If FilesDataGridView.Columns.Contains("Filename") Then
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
    Sub LanguageChangedEventHandler(sender As Object, e As LanguageChangedEventArgs)
        Translator.TranslateControls(Me)
        ReloadGridView()
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the timer reaches the specified time.
    ''' <para>It displays the document preview balloon.</para>
    ''' </summary>
    Private Sub TimerTickEventHandler()
        Try
            _mouseHoverTimer.Enabled = False
            If _mouseHoverRowIndex = -1 Or Not _mouseHoverRowIndex < FilesDataGridView.Rows.Count Then Exit Sub

            Dim rowRectangle = FilesDataGridView.GetRowDisplayRectangle(_mouseHoverRowIndex, True)
            Dim pointX = 0
            Dim pointY = rowRectangle.Y + FilesDataGridView.Top + (rowRectangle.Height / 2)
            Dim alignment As HorizontalAlignment = PaletteHelper.GetTitlebarLocation
            Select Case alignment = HorizontalAlignment.Left
                Case True : pointX = rowRectangle.X + FilesDataGridView.Left + FilesDataGridView.Width + 10
                Case Else : pointX = rowRectangle.X + FilesDataGridView.Left - 10
            End Select
            Dim displayPoint = PointToScreen(New Drawing.Point(pointX, pointY))
            Select Case True
                Case FilesDataGridView.Columns.Contains("Num") : BalloonHelper.ShowFramePreview(FilesDataGridView.Rows(_mouseHoverRowIndex), displayPoint, alignment)
                Case FilesDataGridView.Columns.Contains("Filename") : BalloonHelper.ShowDocumentPreview(FilesDataGridView.Rows(_mouseHoverRowIndex), displayPoint, alignment)
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
        Dim doc = ActiveDocument()
        If IsNothing(doc) Then Exit Sub
        Dim tag = ""
        Dim selectedRows = FilesDataGridView.Rows.ToList.Where(Function(row) row.Selected)
        Dim selectedFiles = selectedRows.Select(Function(row) row.Cells(0).Value.ToString).ToArray
        Select Case GetObjectType(sender)
            Case "ToolStripMenuItem" : tag = DirectCast(sender, ToolStripMenuItem).Tag
            Case "ToolStripComboBox" : tag = DirectCast(sender, ToolStripComboBox).Tag
        End Select
        doc.CancelCommand
        Try
            DocumentEvents.DocumentEventsEnabled = False
            Select Case True
                Case tag = "Filters"
                    Dim comboBox = DirectCast(sender, ToolStripComboBox)
                    comboBox.GetCurrentParent.Hide()
                    If comboBox.SelectedIndex > 0 Then
                        Registerizer.UserSetting("Filters_Selected", comboBox.Text)
                        ReloadGridView()
                    End If
                Case selectedFiles.Count = 0
                Case tag = "Scripts"
                    Dim comboBox = DirectCast(sender, ToolStripComboBox)
                    comboBox.GetCurrentParent.Hide()
                    If comboBox.SelectedIndex > 0 Then
                        ScriptHelper.Start(doc, selectedFiles, comboBox.Text)
                        comboBox.SelectedIndex = 0
                    End If
                Case tag = "Extract"
                    ExtractMethods.ExtractDocuments(selectedFiles)
                Case tag = "Rename"
                    If selectedFiles.Count > 0 Then
                        Dim file = selectedFiles.First

                        Dim dialog = New InputBoxDialog("RenameFile".Translate, IO.Path.GetFileName(file))
                        If dialog.InputText = "" Then Exit Sub

                        Dim newName = FileSystemHelper.RemoveInvalidFileNameCharacters(dialog.InputText.Trim(" ", "."))
                        If newName = "" Or newName.Contains("\") Then Exit Select

                        If Not newName.EndsWith(IO.Path.GetExtension(file)) Then newName &= IO.Path.GetExtension(file)
                        FileSystemHelper.RenameFile(file, "{0}\{1}".Compose(IO.Path.GetDirectoryName(file), newName))
                        PaletteHelper.ReloadFrameList()
                    End If
                Case tag = "Delete"
                    Dim msgRlt = MessageBoxQuestion("DeleteFiles".Translate(selectedFiles.Count))
                    If msgRlt = DialogResult.Yes Then
                        selectedFiles.ToList.ForEach(Sub(file) FileSystemHelper.DeleteFile(file))
                        PaletteHelper.ReloadFrameList()
                    End If
            End Select
            _Loaded = False
            If Not DocumentEvents.DocumentEventsEnabled Then DocumentsHelper.Open(doc.Name)
            If Not _Loaded Then ReloadGridView()
        Catch ex As Exception
            GadecException(ex)
        Finally
            DocumentEvents.DocumentEventsEnabled = True
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
    ''' Clears this palette when no document is active (eg. startscreen of AutoCAD).
    ''' </summary>
    Private Sub ClearPaletteWhenNoActiveDocument()
        FilesDataGridView.Columns.Clear()
        FilesDataGridView.ContextMenuStrip = Nothing
        ltSelectAll.ContextMenuStrip = Nothing
        ltSelectAll.Enabled = False
        OpenFolderButton.Enabled = False
        ltDrawings.Enabled = False
        _toolTip.SetToolTip(OpenFolderButton, "")
        ltDrawings.Text = "Start"
    End Sub

    ''' <summary>
    ''' Clears this palette when the document is not a named drawing (eg. not saved yet).
    ''' </summary>
    Private Sub ClearPaletteWhenNotNamedDrawing()
        FilesDataGridView.Columns.Clear()
        Dim menuData = DataSetHelper.LoadFromXml("{Support}\SetContextMenus.xml".Compose)
        ContextMenuStrip = MenuStripHelper.Create(menuData.GetTable("NotSaved"), AddressOf ContextMenuStripClickEventHandler, My.Resources.ResourceManager)
        FilesDataGridView.ContextMenuStrip = ContextMenuStrip
        ltSelectAll.ContextMenuStrip = ContextMenuStrip
        ltSelectAll.Enabled = False
        OpenFolderButton.Enabled = False
        ltDrawings.Enabled = False
        _toolTip.SetToolTip(OpenFolderButton, "")
        ltDrawings.Text = "Not saved"
    End Sub

End Class
