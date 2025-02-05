'Gadec Engineerings Software (c) 2022
Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms
Imports GadecCAD_NG.Extensions

''' <summary>
''' <para><see cref="DesignCenter"/> allows the user to select a item (symbol, frame, or drawing part) to insert into the current drawing.</para>
''' <para>Items are divided into modules and pages.</para>
''' </summary>
Public Class DesignCenter
    ''' <summary>
    ''' The current scale of the drawing.
    ''' </summary>
    Private _insertScale As Double
    ''' <summary>
    ''' A timer to delay the display of the image balloon.
    ''' </summary>
    Private _mouseHoverTimer As Timer
    ''' <summary>
    ''' Keeps the index of the row of the framelist that the mouse hovers over.
    ''' </summary>
    Private _mouseHoverRowIndex As Integer = -1

    ''' <summary>
    ''' A database that contains a record for each module.
    ''' </summary>
    Private _moduleData As DataTable
    ''' <summary>
    ''' A database that contains a record for each page of the selected module.
    ''' </summary>
    Private _pageData As DataTable
    ''' <summary>
    ''' A database that contains a record for each item of the selected page.
    ''' </summary>
    Private _itemData As DataTable
    ''' <summary>
    ''' The key of the selected module.
    ''' </summary>
    Private _currentModule As String
    ''' <summary>
    ''' The key of the selected page.
    ''' </summary>
    Private _currentPage As String
    ''' <summary>
    ''' The key of the selected item.
    ''' </summary>
    Private _currentItem As String
    ''' <summary>
    ''' The name of the selected module and page.
    ''' </summary>
    Private _currentModuleAndPage As String
    ''' <summary>
    ''' The fullname of the sourcefile that contains the current selected item.
    ''' </summary>
    Private _sourceFile As String
    ''' <summary>
    ''' A list of images associated with the currently selected page.
    ''' </summary>
    Private _pageImages As Dictionary(Of String, Bitmap)

    ''' <summary>
    ''' Contains the current/previous height of the dialogbox.
    ''' </summary>
    Private _formHeight As Integer
    ''' <summary>
    ''' Contains the current/previous width of the dialogbox.
    ''' </summary>
    Private _formWidth As Integer

    ''' <summary>
    ''' Determines if the modulelist is fully loaded.
    ''' </summary>
    Private _modulesLoaded As Boolean
    ''' <summary>
    ''' Determines if the pagelist is fully loaded.
    ''' </summary>
    Private _pagesLoaded As Boolean
    ''' <summary>
    ''' Determines if the itempage is fully loaded.
    ''' </summary>
    Private _itemsLoaded As Boolean

    ''' <summary>
    ''' Determines if this dialogbox is fully loaded.
    ''' </summary>
    Private ReadOnly _dialogLoaded As Boolean = False
    ''' <summary>
    ''' An ini-string to show the selected language in the lists and page.
    ''' </summary>
    Private ReadOnly _languageIni As String = "{0}=100%".Compose(Translator.Selected)
    ''' <summary>
    ''' The tooltip to use on this dialogbox.
    ''' </summary>
    Private ReadOnly _toolTip As New ToolTip

    ''' <summary>
    ''' A grey color to use on this dialogbox.
    ''' </summary>
    Private ReadOnly _colorGrey As Color = Color.FromArgb(50, 50, 60)
    ''' <summary>
    ''' An almost black color to use on this dialogbox.
    ''' </summary>
    Private ReadOnly _colorAlmostBlack As Color = Color.FromArgb(15, 15, 15)
    ''' <summary>
    ''' An orange color to use on this dialogbox.
    ''' </summary>
    Private ReadOnly _colorOrange As Color = Color.FromArgb(240, 160, 0)
    ''' <summary>
    ''' The SystemWindow color to use on this dialogbox.
    ''' </summary>
    Private ReadOnly _colorSystemWindow As Color = SystemColors.Window
    ''' <summary>
    ''' The tabstop format of DataGridViewCells.
    ''' </summary>
    Private ReadOnly _itemsDataGridViewStringFormat As New StringFormat

    'form

    ''' <summary>
    ''' Initializes a new instance of <see cref="DesignCenter"/>.
    ''' <para><see cref="DesignCenter"/> allows the user to select a item (symbol, frame, or drawing part) to insert into the current drawing.</para>
    ''' <para>Items are divided into modules and pages.</para>
    ''' </summary>
    ''' <param name="session">Previous session settings, namely module, page and item.</param>
    ''' <param name="insertScale">The current scale of the drawing.</param>
    Sub New(session As String(), insertScale As Double)
        ' This call is required by the designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.

        Me.Text = Registerizer.GetApplicationVersion()
        Translator.TranslateControls(Me)

        _itemsDataGridViewStringFormat.SetTabStops(0F, {100.0F, 100.0F, 100.0F})

        _currentModule = session(0)
        _currentPage = session(1)
        _currentItem = session(2)
        _insertScale = insertScale

        _toolTip.AutomaticDelay = 500
        _formHeight = Me.Height
        _formWidth = Me.Width
        _dialogLoaded = True
        Me.Height = 0.6 * ApplicationHelper.WindowSize.Height
        Me.ShowDialog()
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the dialogbox is loading.
    ''' <para>It wires up the dialog, select the previous module and calls the method for doing so for the page.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Me_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            ScaleComboBox.Text = "1:{0}".Compose(Format(_insertScale, "0.##"))
            Dim scales = DataSetHelper.LoadFromXml("{Support}\SetStandards.xml".Compose).GetTable("Scales").GetStringsFromColumn("Name").ToList
            If scales.Count > 0 Then scales.RemoveAt(0)
            ScaleComboBox.Items.AddRange(scales.ToArray)

            _moduleData = DataSetHelper.LoadFromXml("{Support}\SetDesignCenter.xml".Compose).GetTable("Modules", "Name")
            If NotNothing(_moduleData) Then
                ModulesDataGridView.DataSource = _moduleData
                ModulesDataGridView.SetColumnWidths(_languageIni.Cut.ToIniDictionary)
                For Each row In ModulesDataGridView.Rows.ToArray
                    If row.GetString("Name") = _currentModule Then
                        ModulesDataGridView.CurrentCell = row.Cells(Translator.Selected)
                        Exit For
                    End If
                Next
            End If
            _modulesLoaded = True
            PagesDataGridView_SelectionChanged(Nothing, Nothing)
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the dialogbox gets the focus.
    ''' <para>It sets the focus to the OK button.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Me_Activated(sender As Object, e As EventArgs) Handles Me.Activated
        Try
            ltOK.Focus()
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when user clicks on an empty part of this dialogbox.
    ''' <para>If screenshots are enabled (by doubleclicking on the languageflag, see <see cref="SettingsPalette"/>) a screenshot of this dialogbox will be taken.</para>
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
    ''' EventHandler for the event that occurs when the user resizes this dialogbox.
    ''' <para>It handles the size and position of controls it contains.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Me_SizeChanged(sender As Object, e As EventArgs) Handles Me.SizeChanged
        Try
            If Not _dialogLoaded Then Exit Sub

            Me.Height = {420, Me.Height}.Max
            Me.Width = {600, Me.Width}.Max
            Dim heightDifference = Me.Height - _formHeight
            If Not heightDifference = 0 Then
                Dim a = (ModulesDataGridView.Height + PagesDataGridView.Height) + heightDifference
                Dim b = Int(a / 4)
                Dim c = a - b
                ModulesDataGridView.Height = b
                PagesDataGridView.Height = c
                PagesDataGridView.Top = ModulesDataGridView.Top + b + 6
                ltOK.Top += heightDifference
                ltCancel.Top += heightDifference
                ItemsDataGridView.Height += heightDifference
                PicturePanel.Height += heightDifference
                ScaleComboBox.Top += heightDifference
                ltScale.Top += heightDifference
                ltDescription.Top += heightDifference
            End If
            Dim widthDifference = Me.Width - _formWidth
            If Not widthDifference = 0 Then
                ItemsDataGridView.Width += widthDifference
                PicturePanel.Width += widthDifference
                ltDescription.Width += widthDifference
                ltCancel.Left += widthDifference
                ltOK.Left += widthDifference
                ScaleComboBox.Left += widthDifference
                ltScale.Left += widthDifference
            End If
            _formHeight = Me.Height
            _formWidth = Me.Width
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    'functions

    ''' <summary>
    ''' Gets the current session settings, namely module, page and item.
    ''' </summary>
    ''' <returns>A list representing the session.</returns>
    Function GetSession() As String()
        Return {_currentModule, _currentPage, _currentItem}
    End Function

    ''' <summary>
    ''' Gets the possibly changed scale (there is a combobox that allows the user to change it).
    ''' </summary>
    ''' <returns>The scale.</returns>
    Function GetInsertScale() As Double
        Return _insertScale
    End Function

    ''' <summary>
    ''' Gets a setting from the designcenter database that determines whether the selected item should be scaled during insertion.
    ''' </summary>
    ''' <returns>If true it must be scaled like the drawingscale.</returns>
    Function GetAllowScale() As Boolean
        If ItemsDataGridView.SelectedRows.Count = 0 Then Return False

        Return ItemsDataGridView.SelectedRows(0).GetString("Scale") = "Yes"
    End Function

    ''' <summary>
    ''' Gets a setting from the designcenter database that determines whether the selected item may be rotated by the user during insertion.
    ''' </summary>
    ''' <returns>If true it can be rotated.</returns>
    Function GetAllowRotation() As Boolean
        If ItemsDataGridView.SelectedRows.Count = 0 Then Return False

        Return ItemsDataGridView.SelectedRows(0).GetString("Rotation") = "Yes"
    End Function

    ''' <summary>
    ''' Gets the fullname of the sourcefile that contains the current selected item.
    ''' </summary>
    ''' <returns>The source-filename.</returns>
    Function GetSourceFile() As String
        Return _sourceFile
    End Function

    ''' <summary>
    ''' Gets the blockname of the currently selected item.
    ''' </summary>
    ''' <returns>The blockname.</returns>
    Function GetBlockName() As String
        If ItemsDataGridView.SelectedRows.Count = 0 Then Return ""

        Return ItemsDataGridView.SelectedRows(0).GetString("BlockName")
    End Function

    ''' <summary>
    ''' Gets the descriptions (multiple languages) of the current selected item.
    ''' </summary>
    ''' <returns>The descriptions.</returns>
    Function GetDescriptions() As String()
        If ItemsDataGridView.SelectedRows.Count = 0 Then Return Nothing

        Dim cells = ItemsDataGridView.SelectedRows(0).Cells.ToList
        Return cells.Select(Function(cell) "{0}={1}".Compose(cell.OwningColumn.Name, cell.Value.ToString)).ToArray
    End Function

    'buttons

    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks the OK button, doubleclicks a cell in the itemslist, presses the enterkey or doubleclicks a picture.
    ''' <para>The last two are derived from the respective events.</para>
    ''' <para>If selection is valid, it sets the button value closes the dialogbox.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub AcceptButton_Click(sender As Object, e As EventArgs) Handles ltOK.Click, ItemsDataGridView.CellMouseDoubleClick
        Try
            If ItemsDataGridView.SelectedRows.Count > 0 Then
                Me.DialogResult = DialogResult.OK
                Me.Hide()
            End If
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks the Cancel button.
    ''' <para>It sets the buttonvalue, saves the settings to the registry and closes the dialogbox.</para>
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

    'enterkey

    ''' <summary>
    ''' EventHandler for the event that occurs when the user presses a key.
    ''' <para>It only accepts the enter key and if it does, it sets the button value as if the OK button was pressed and closes the dialogbox.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub DataGridViews_KeyDown(sender As Object, e As KeyEventArgs) Handles ItemsDataGridView.KeyDown, ModulesDataGridView.KeyDown, PagesDataGridView.KeyDown
        Try
            If Not e.KeyCode = Keys.Enter Then Exit Sub

            e.SuppressKeyPress = True
            AcceptButton_Click(Nothing, Nothing)
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    'datagridview modules

    ''' <summary>
    ''' EventHandler for the event that occurs when the user selects a row in the moduleslist.
    ''' <para>It loads the selected module.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ModulesDataGridView_SelectionChanged(sender As Object, e As EventArgs) Handles ModulesDataGridView.SelectionChanged
        Try
            If ModulesDataGridView.SelectedRows.Count = 0 Then Exit Sub

            If _modulesLoaded Then _currentModule = ModulesDataGridView.SelectedRows(0).GetString("Name")
            _pagesLoaded = False
            PagesDataGridView.DataSource = Nothing
            _pagesLoaded = True
            Dim fileName = ModulesDataGridView.SelectedRows(0).GetString("File")
            _pageData = DataSetHelper.LoadFromXml("{Support}\{0}".Compose(fileName)).GetTable("Pages", "Name")
            Select Case IsNothing(_pageData)
                Case True
                    UndoItemSelection(_currentItem)
                    ItemsDataGridView.Visible = True
                    ItemsDataGridView.DataSource = Nothing
                Case Else
                    _pagesLoaded = False
                    PagesDataGridView.DataSource = _pageData
                    PagesDataGridView.SetColumnWidths(_languageIni.Cut.ToIniDictionary)
                    _pagesLoaded = True
                    For Each row In PagesDataGridView.Rows.ToArray
                        If row.GetString("Name") = _currentPage Then
                            PagesDataGridView.CurrentCell = row.Cells(Translator.Selected)
                            Exit For
                        End If
                    Next
                    If _currentPage = "" Then _currentPage = PagesDataGridView.Rows(0).GetString("Name")
                    PagesDataGridView_SelectionChanged(Nothing, Nothing)
            End Select
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user hovers the mouse over the moduleslist.
    ''' <para>It set the focus on the this list.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ModulesDataGridView_MouseEnter(sender As Object, e As EventArgs) Handles ModulesDataGridView.MouseEnter
        Try
            ModulesDataGridView.Focus()
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user hovers the mouse into a cell of the moduleslist.
    ''' <para>It highlights the row.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ModulesDataGridView_CellMouseEnter(sender As Object, e As DataGridViewCellEventArgs) Handles ModulesDataGridView.CellMouseEnter
        Try
            If Registerizer.UserSetting("HidePreviews") = "True" Or e.RowIndex < 0 Then Exit Sub

            ModulesDataGridView.Rows(e.RowIndex).HighlightRow(True)
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user hovers the mouse out of a cell of the moduleslist.
    ''' <para>It no longer highlights the row.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ModulesDataGridView_CellMouseLeave(sender As Object, e As DataGridViewCellEventArgs) Handles ModulesDataGridView.CellMouseLeave
        Try
            If Registerizer.UserSetting("HidePreviews") = "True" Or e.RowIndex < 0 Then Exit Sub

            ModulesDataGridView.Rows(e.RowIndex).HighlightRow(False)
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    'datagridview pages 

    ''' <summary>
    ''' EventHandler for the event that occurs when the user selects a row in the pageslist.
    ''' <para>It shows the selected page in the items area.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub PagesDataGridView_SelectionChanged(sender As Object, e As EventArgs) Handles PagesDataGridView.SelectionChanged
        Try
            If Not _modulesLoaded Then Exit Sub
            If Not _pagesLoaded Then Exit Sub
            If PagesDataGridView.SelectedRows.Count = 0 Then Exit Sub

            _currentPage = PagesDataGridView.SelectedRows(0).GetString("Name")
            _itemsLoaded = False
            ItemsDataGridView.DataSource = Nothing
            _itemsLoaded = True
            _itemData = _pageData.GetTable("Blocks")
            _itemData.AssignPrimaryKey("Page;Num")
            Dim itemPageRows = _itemData.Select("Page='{0}'".Compose(_currentPage), "Num")
            UndoItemSelection(_currentItem)
            If itemPageRows.Count = 0 Then ItemsDataGridView.Visible = True : Exit Sub

            Dim itemPageData = itemPageRows.CopyToDataTable
            Dim isTextPage = Not PagesDataGridView.SelectedRows(0).GetString("Type") = "Pictures"
            If Not _currentModuleAndPage = "{0}_{1}".Compose(_currentModule, _currentPage) Then
                _currentModuleAndPage = "{0}_{1}".Compose(_currentModule, _currentPage)
                _sourceFile = PagesDataGridView.SelectedRows(0).GetString("SourceFile")
                Dim blockNames = itemPageData.GetUniqueStringsFromColumn("BlockName")
                Dim color = If(isTextPage, Drawing.Color.Empty, Drawing.Color.Black)
                _pageImages = XRecordImagesHelper.Load("{Resources}\{0}".Compose(_sourceFile), "{Company}".Compose, "BlockPics", blockNames, color)
            End If
            _itemsLoaded = False
            ItemsDataGridView.DataSource = itemPageData
            ItemsDataGridView.SetColumnWidths(_languageIni.Cut.ToIniDictionary)
            _itemsLoaded = True
            For Each row In ItemsDataGridView.Rows.ToArray
                row.Cells(Translator.Selected).Value = row.GetString(Translator.Selected).Compose
                If row.GetString("Num") = _currentItem Then ItemsDataGridView.CurrentCell = row.Cells(Translator.Selected)
            Next
            ItemsDataGridView.Visible = isTextPage
            If Not isTextPage Then LoadPicturePanel()
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user hovers the mouse over the pageslist.
    ''' <para>It set the focus on the this list.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub PagesDataGridView_MouseEnter(sender As Object, e As EventArgs) Handles PagesDataGridView.MouseEnter
        Try
            PagesDataGridView.Focus()
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user hovers the mouse into a cell of the pageslist.
    ''' <para>It highlights the row.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub PagesDataGridView_CellMouseEnter(sender As Object, e As DataGridViewCellEventArgs) Handles PagesDataGridView.CellMouseEnter
        Try
            If Registerizer.UserSetting("HidePreviews") = "True" Or e.RowIndex < 0 Then Exit Sub

            PagesDataGridView.Rows(e.RowIndex).HighlightRow(True)
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user hovers the mouse out of a cell of the pageslist.
    ''' <para>It no longer highlights the row.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub PagesDataGridView_CellMouseLeave(sender As Object, e As DataGridViewCellEventArgs) Handles PagesDataGridView.CellMouseLeave
        Try
            If Registerizer.UserSetting("HidePreviews") = "True" Or e.RowIndex < 0 Then Exit Sub

            PagesDataGridView.Rows(e.RowIndex).HighlightRow(False)
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    'datagridview items

    ''' <summary>
    ''' EventHandler for the event that occurs when the cells get painted.
    ''' <para>The cells are painted with the included tab stops.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ItemsDataGridView_CellPainting(ByVal sender As Object, ByVal e As DataGridViewCellPaintingEventArgs) Handles ItemsDataGridView.CellPainting
        If IsNothing(e.Value) Then Exit Sub

        e.Paint(e.CellBounds, DataGridViewPaintParts.All And Not DataGridViewPaintParts.ContentForeground)
        Dim textColor = If(ItemsDataGridView.SelectedCells.Contains(ItemsDataGridView.Rows(e.RowIndex).Cells(e.ColumnIndex)), Color.White, SystemColors.ControlText)
        e.Graphics.DrawString(e.Value.ToString(), e.CellStyle.Font, New SolidBrush(textColor), e.CellBounds.X, CInt(e.CellBounds.Y + 1.5), _itemsDataGridViewStringFormat)
        e.Handled = True
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user selects a row in the itemslist.
    ''' <para>It shows data of the selected item (lowerleft corner) and disables the scale selection if necessary.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ItemsDataGridView_SelectionChanged(sender As Object, e As EventArgs) Handles ItemsDataGridView.SelectionChanged
        Try
            If ItemsDataGridView.SelectedRows.Count = 0 Then Exit Sub

            ltDescription.Text = "{0} ({1})".Compose(IO.Path.GetFileNameWithoutExtension(_sourceFile), ItemsDataGridView.SelectedRows(0).Cells("BlockName").Value.ToString)
            If _itemsLoaded And _modulesLoaded Then _currentItem = ItemsDataGridView.SelectedRows(0).GetString("Num")
            Select Case ItemsDataGridView.SelectedRows(0).GetString("Scale") = "Yes"
                Case True : ScaleComboBox.Enabled = True : ltScale.Enabled = True : ScaleComboBox.Text = "1:{0}".Compose(Format(_insertScale, "0.##"))
                Case Else : ScaleComboBox.Enabled = False : ltScale.Enabled = False : ScaleComboBox.Text = "1:1"
            End Select
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user hovers the mouse over the itemslist.
    ''' <para>It set the focus on this list.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ItemsDataGridView_MouseEnter(sender As Object, e As EventArgs) Handles ItemsDataGridView.MouseEnter
        Try
            ItemsDataGridView.Focus()
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user hovers the mouse into a cell of the itemslist.
    ''' <para>It highlights the row and starts a timer that displays the image balloon in a delayed manner.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ItemsDataGridView_CellMouseEnter(sender As Object, e As DataGridViewCellEventArgs) Handles ItemsDataGridView.CellMouseEnter
        Try
            If NotNothing(_mouseHoverTimer) Then _mouseHoverTimer.Dispose() : _mouseHoverTimer = Nothing
            If Registerizer.UserSetting("HidePreviews") = "True" Or e.RowIndex < 0 Then Exit Sub

            _mouseHoverRowIndex = e.RowIndex
            _mouseHoverTimer = New Timer
            AddHandler _mouseHoverTimer.Tick, AddressOf TimerTickEventHandler
            _mouseHoverTimer.Interval = 350
            _mouseHoverTimer.Enabled = True
            ItemsDataGridView.Rows(e.RowIndex).HighlightRow(True)
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user hovers the mouse out of a cell of the itemslist.
    ''' <para>It no longer highlights the row and stops the timer (for displaying the image balloon).</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ItemsDataGridView_CellMouseLeave(sender As Object, e As DataGridViewCellEventArgs) Handles ItemsDataGridView.CellMouseLeave
        Try
            If NotNothing(_mouseHoverTimer) Then _mouseHoverTimer.Enabled = False
            If Registerizer.UserSetting("HidePreviews") = "True" Or e.RowIndex < 0 Then Exit Sub

            BalloonHelper.ShowBlockPreview("", Nothing, Nothing)
            ItemsDataGridView.Focus()
            ItemsDataGridView.Rows(e.RowIndex).HighlightRow(False)
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    'picturepanel section

    ''' <summary>
    ''' EventHandler for the event that occurs when the user hovers the mouse over the items picturepanel.
    ''' <para>It set the focus on this panel.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub PicturePanel_MouseEnter(sender As Object, e As EventArgs) Handles PicturePanel.MouseEnter
        Try
            PicturePanel.Focus()
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    'scale section

    ''' <summary>
    ''' EventHandler for the event that occurs when the combobox loses focus.
    ''' <para>It transposes the scale display.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ScaleComboBox_LostFocus(sender As Object, e As EventArgs) Handles ScaleComboBox.LostFocus
        Try
            ScaleComboBox.Text = "1:{0}".Compose(Format(_insertScale, "0.##"))
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user changes the text in the combobox by typing or selecting.
    ''' <para>It determines and stores the provided scaling factor.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ScaleComboBox_TextChanged(sender As Object, e As EventArgs) Handles ScaleComboBox.TextChanged
        Try
            Dim settings = " =;1:=;,=.".Cut.ToIniDictionary
            Dim scale = ScaleComboBox.Text.ReplaceMultiple(settings)
            If ScaleComboBox.Enabled = True Then _insertScale = scale.ToDouble
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    'private eventHandlers

    ''' <summary>
    ''' EventHandler for the event that occurs when the timer reaches the specified time.
    ''' <para>It displays the image balloon.</para>
    ''' </summary>
    Private Sub TimerTickEventHandler()
        Try
            If Not ItemsDataGridView.Visible Then Exit Sub

            _mouseHoverTimer.Enabled = False
            If _mouseHoverRowIndex = -1 Or Not _mouseHoverRowIndex < ItemsDataGridView.Rows.Count Then Exit Sub

            Dim rowRectangle = ItemsDataGridView.GetRowDisplayRectangle(_mouseHoverRowIndex, True)
            Dim pointX = rowRectangle.X + ItemsDataGridView.Left + ItemsDataGridView.Width * 3 / 4
            Dim pointY = rowRectangle.Y + ItemsDataGridView.Top + (rowRectangle.Height / 2)
            Dim displayPoint = PointToScreen(New Drawing.Point(pointX, pointY))
            Dim blockName = ItemsDataGridView.Rows(_mouseHoverRowIndex)?.GetString("BlockName")
            BalloonHelper.ShowBlockPreview(blockName, displayPoint, _pageImages)
            ItemsDataGridView.Focus()
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks on a picture.
    ''' <para>It highlights the selected picture, shows data of the selected item (lowerleft corner) and disables the scale selection if necessary.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub PictureClickEventHandler(sender As Object, e As EventArgs)
        Try
            UndoItemSelection(_currentItem)
            SetItemSelection(sender.Tag)
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user doubleclicks on a picture.
    ''' <para>It selects the item, sets the button value as if the OK button was pressed and closes the dialogbox.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub PictureDoubleClickEventHandler(sender As Object, e As EventArgs)
        Try
            UndoItemSelection(_currentItem)
            SetItemSelection(sender.Tag)
            AcceptButton_Click(Nothing, Nothing)
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    'private subs

    ''' <summary>
    ''' Loads/rearrange the picturepanel and loads itemdata and the respective images.
    ''' </summary>
    Private Sub LoadPicturePanel()
        Me.PicturePanel.SuspendLayout()
        PicturePanel.AutoScrollPosition = New Point(0, 0)
        Dim font = FontHelper.ArialNarrowRegular(9.75!)

        Dim controlsToRemove = PicturePanel.Controls.ToList
        Dim countX = -1
        Dim countY = 0
        Dim pictureHeight = 63
        Dim pictureWidth = 89
        For i = 0 To ItemsDataGridView.Rows.Count - 1
            countX += 1
            If countX > 4 Then countX = 0 : countY += 1
            Dim row = ItemsDataGridView.Rows(i)
            If row.GetString("BlockName") = "" Then Continue For

            Dim key = row.GetString("Num")
            Dim pictureBox As PictureBox
            Dim pictureName = "Picture{0}".Compose(key)
            Select Case PicturePanel.Controls.ContainsKey(pictureName)
                Case True
                    pictureBox = PicturePanel.Controls(pictureName)
                    controlsToRemove.Remove(pictureBox)
                Case Else
                    pictureBox = New PictureBox
                    PicturePanel.Controls.Add(pictureBox)
                    With pictureBox
                        .Name = pictureName
                        .SizeMode = PictureBoxSizeMode.CenterImage
                        .Left = 3 + (countX * (pictureWidth + 8))
                        .Top = 6 + (countY * (pictureHeight + 24))
                        .Width = pictureWidth
                        .Height = pictureHeight
                        .Tag = key
                        .BackColor = _colorGrey
                        AddHandler .Click, AddressOf PictureClickEventHandler
                        AddHandler .DoubleClick, AddressOf PictureDoubleClickEventHandler
                    End With
            End Select
            Select Case True
                Case IsNothing(_pageImages) : pictureBox.Image = Nothing
                Case Not _pageImages.ContainsKey(row.GetString("BlockName")) : pictureBox.Image = Nothing
                Case Else : pictureBox.Image = _pageImages(row.GetString("BlockName"))
            End Select
            _toolTip.SetToolTip(pictureBox, row.GetTranslation)
            Dim label As Label
            Dim labelName = "Label{0}".Compose(key)
            Select Case PicturePanel.Controls.ContainsKey(labelName)
                Case True
                    label = PicturePanel.Controls(labelName)
                    controlsToRemove.Remove(label)
                Case Else
                    label = New Label
                    PicturePanel.Controls.Add(label)
                    With label
                        .Name = labelName
                        .AutoSize = False
                        .TextAlign = ContentAlignment.BottomCenter
                        .Font = font
                        .Left = 3 + (countX * (pictureWidth + 8))
                        .Top = 3 + (countY * (pictureHeight + 24))
                        .Width = pictureWidth
                        .Height = pictureHeight + 18
                        .Tag = key
                        AddHandler .Click, AddressOf PictureClickEventHandler
                        AddHandler .DoubleClick, AddressOf PictureDoubleClickEventHandler
                    End With
            End Select
            label.Text = row.GetString("BlockName")
        Next

        For Each control In controlsToRemove
            PicturePanel.Controls.Remove(control)
            control.Dispose()
        Next

        Dim pictureSelected = "001"
        For Each key In ItemsDataGridView.GetStringsFromColumn("Num")
            If PicturePanel.Controls.ContainsKey("Picture{0}".Compose(key)) Then
                If key > _currentItem Then Exit For
                pictureSelected = key
            End If
        Next
        SetItemSelection(pictureSelected)
        PicturePanel.ResumeLayout()
    End Sub

    ''' <summary>
    ''' Highlights the (selected) item with the specified key in the pictuepanel.
    ''' </summary>
    ''' <param name="key">The key of the item.</param>
    Private Sub SetItemSelection(key As String)
        Dim label = Me.PicturePanel.Controls("Label{0}".Compose(key))
        If NotNothing(label) Then label.BackColor = _colorOrange
        Dim pictureBox = Me.PicturePanel.Controls("Picture{0}".Compose(key))
        If NotNothing(pictureBox) Then pictureBox.BackColor = _colorAlmostBlack
        Dim gridRow = ItemsDataGridView.GetRowByValue("Num", key)
        If NotNothing(gridRow) Then ItemsDataGridView.CurrentCell = gridRow.Cells(Translator.Selected)
        _currentItem = key
    End Sub

    ''' <summary>
    ''' Undo the highlight of the (previously selected) item with the specified key in the pictuepanel.
    ''' </summary>
    ''' <param name="key">The key of the item.</param>
    Private Sub UndoItemSelection(key As String)
        Dim label = DirectCast(Me.PicturePanel.Controls("Label{0}".Compose(key)), Label)
        If NotNothing(label) Then label.BackColor = _colorSystemWindow
        Dim pictureBox = DirectCast(Me.PicturePanel.Controls("Picture{0}".Compose(key)), PictureBox)
        If NotNothing(pictureBox) Then pictureBox.BackColor = _colorGrey
    End Sub

End Class
