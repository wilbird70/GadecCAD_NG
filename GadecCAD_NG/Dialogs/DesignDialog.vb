'Gadec Engineerings Software (c) 2022
Imports System.Data
Imports System.Windows.Forms

''' <summary>
''' <para><see cref="DesignDialog"/> allows the user to provide data for following commands:</para>
''' <para>- Replace Frames.</para>
''' <para>- Design Array.</para>
''' <para>- Design Fire-detectors.</para>
''' <para>- Design Wallmount-devices.</para>
''' </summary>
Public Class DesignDialog
    ''' <summary>
    ''' The current scale of the drawing.
    ''' </summary>
    Private _insertScale As Double
    ''' <summary>
    ''' The index of the selected item.
    ''' </summary>
    Private _selectedIndex As Integer
    ''' <summary>
    ''' The fullname of the sourcefile that contains the currently selected item.
    ''' </summary>
    Private _sourceFile As String
    ''' <summary>
    ''' The blockname of the selected item.
    ''' </summary>
    Private _blockName As String
    ''' <summary>
    ''' The insert rotation for the currently selected item.
    ''' </summary>
    Private _rotation As Double
    ''' <summary>
    ''' A database stored in the registry with data for itemsdata for each command.
    ''' </summary>
    Private _userData As DataTable

    ''' <summary>
    ''' The commandcode.
    ''' </summary>
    Private ReadOnly _command As String
    ''' <summary>
    ''' The tooltip to use on this dialogbox.
    ''' </summary>
    Private ReadOnly _toolTip As New ToolTip

    'form

    ''' <summary>
    ''' Initializes a new instance of <see cref="DesignDialog"/>.
    ''' <para><see cref="DesignDialog"/> allows the user to provide data for following commands:</para>
    ''' <para>- Replace Frames, command="DF".</para>
    ''' <para>- Design Array, command="DA".</para>
    ''' <para>- Design Fire-detectors, command="DD".</para>
    ''' <para>- Design Wallmount-devices, command="DW".</para>
    ''' </summary>
    ''' <param name="command">The commandcode.</param>
    ''' <param name="insertScale">The current scale of the drawing.</param>
    Sub New(command As String, insertScale As Double)
        ' This call is required by the designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.

        Me.Text = Registerizer.GetApplicationVersion()
        Translator.TranslateControls(Me)

        _command = command
        _insertScale = insertScale
        Select Case _command
            Case "DA", "DF" : _userData = Registerizer.UserData("Frames-DataTable")
            Case "DD" : _userData = Registerizer.UserData("Detectors-DataTable")
            Case "DW" : _userData = Registerizer.UserData("Wallmounts-DataTable")
        End Select

        For Each control In Me.Controls.ToArray
            Select Case True
                Case control.Name.EndsWith("DA") : If Not _command = "DA" Then control.Visible = False
                Case control.Name.EndsWith("DF") : If Not _command = "DF" Then control.Visible = False
                Case control.Name.EndsWith("DW") : If Not _command = "DW" Then control.Visible = False
            End Select
        Next
        Me.ShowDialog()
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the dialogbox is loading.
    ''' <para>It wires up the dialog.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Me_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            AddPictureBox()
            AddItemsToListBox()
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

    'functions

    ''' <summary>
    ''' Gets the possibly changed scale (it can be changed when user clicks the image to change selected item).
    ''' </summary>
    ''' <returns>The scale.</returns>
    Function GetInsertScale() As Double
        Return _insertScale
    End Function

    ''' <summary>
    ''' Gets the index of the currently selected item.
    ''' </summary>
    ''' <returns>Index of the selected item.</returns>
    Function GetSelectedIndex() As Integer
        Return _selectedIndex
    End Function

    ''' <summary>
    ''' Gets the name of the currently selected item.
    ''' </summary>
    ''' <returns>Name the selected item.</returns>
    Function GetSelectedItem() As String
        Return _userData.Rows(_selectedIndex).GetString("Name")
    End Function

    ''' <summary>
    ''' Gets the fullname of the sourcefile that contains the currently selected item.
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
        Return _blockName
    End Function

    ''' <summary>
    ''' Gets the users insertrotation for the currently selected item.
    ''' </summary>
    ''' <returns>If true it can be rotated.</returns>
    Function GetRotation() As Double
        Return _rotation
    End Function

    ''' <summary>
    ''' Gets the arraysettings for the Design Array Command.
    ''' </summary>
    ''' <returns></returns>
    Function GetArraySettings() As String()
        Dim output = "{0};{1};{2};{3}".Compose(ColumnsTextBoxDA.Text, RowsTextBoxDA.Text, ColumnDistanceTextBoxDA.Text, RowDistanceTextBoxDA.Text)
        Return output.Cut
    End Function

    'buttons

    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks the OK button or doubleclicks an item in the list.
    ''' <para>It sets the buttonvalue and closes the dialogbox.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub AcceptButton_Click(sender As Object, e As EventArgs) Handles ltOK.Click, ItemsListBox.DoubleClick
        Try
            If _userData.Rows(_selectedIndex).GetString("Name") = "Symbol" Then
                Dim output = Strings.Join(GetArraySettings, ";")
                Registerizer.UserSetting("SymbolArraySettings", output)
            End If
            Me.DialogResult = DialogResult.OK
            Me.Hide()
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
            If _userData.Rows(_selectedIndex).GetString("Name") = "Symbol" Then
                Dim output = Strings.Join(GetArraySettings, ";")
                Registerizer.UserSetting("SymbolArraySettings", output)
            End If
            Me.Hide()
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks the RotateToRight button.
    ''' <para>It rotates the image clockwise and changes the user rotationsetting for this item as well.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub RotateToRightButtonDW_Click(sender As Object, e As EventArgs) Handles RotateToRightButtonDW.Click
        Try
            Dim pictureBox = DirectCast(Me.Controls("PictureBox"), PictureBox)
            Dim bitmap = New Drawing.Bitmap(pictureBox.Image)
            bitmap.RotateFlip(Drawing.RotateFlipType.Rotate270FlipNone)
            pictureBox.Image = bitmap

            Dim userRow = _userData.Rows(_selectedIndex)
            _rotation = Val(userRow.GetString("Rotation")) + 90
            If _rotation > 270 Then _rotation = 0
            userRow.SetString("Rotation", _rotation.ToString)
            Registerizer.UserData("Wallmounts-DataTable", _userData)
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks the RotateToLeft button.
    ''' <para>It rotates the image counterclockwise and changes the user rotationsetting for this item as well.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub RotateToLeftButtonDW_Click(sender As Object, e As EventArgs) Handles RotateToLeftButtonDW.Click
        Try
            Dim pictureBox = DirectCast(Me.Controls("PictureBox"), PictureBox)
            Dim bitmap = New Drawing.Bitmap(pictureBox.Image)
            bitmap.RotateFlip(Drawing.RotateFlipType.Rotate90FlipNone)
            pictureBox.Image = bitmap

            Dim userRow = _userData.Rows(_selectedIndex)
            _rotation = Val(userRow.GetString("Rotation")) + 90
            If _rotation < 0 Then _rotation = 270
            userRow.SetString("Rotation", _rotation.ToString)
            Registerizer.UserData("Wallmounts-DataTable", _userData)
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks the FactorySettings button.
    ''' <para>It loads the standard database for the current command.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub FactorySettingsButton_Click(sender As Object, e As EventArgs) Handles FactorySettingsButton.Click
        Try
            Select Case _command
                Case "DA", "DF" : Main.SetGadecFactorySettings("Frames") : _userData = Registerizer.UserData("Frames-DataTable")
                Case "DD" : Main.SetGadecFactorySettings("Detectors") : _userData = Registerizer.UserData("Detectors-DataTable")
                Case "DW" : Main.SetGadecFactorySettings("Wallmounts") : _userData = Registerizer.UserData("Wallmounts-DataTable")
            End Select
            AddItemsToListBox()
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    'listboxes

    ''' <summary>
    ''' EventHandler for the event that occurs when the user selects an item in the itemslist.
    ''' <para>It shows data and image of the selected item.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ItemsListBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ItemsListBox.SelectedIndexChanged
        Try
            Dim index = ItemsListBox.SelectedIndex
            Registerizer.UserSetting("DDselected{0}".Compose(_command), index)
            LoadSelectedItem(index)
            _selectedIndex = index
            If Not _command = "DA" OrElse index < 0 Then Exit Sub

            Dim userRow = _userData.Rows(index)
            Select Case userRow.GetString("Name") = "Symbol"
                Case True 'symbol
                    Dim arraySettings = "{0};;;".Compose(Registerizer.UserSetting("SymbolArraySettings")).Cut
                    ColumnsTextBoxDA.Text = If(arraySettings(0) = "", "5", arraySettings(0))
                    RowsTextBoxDA.Text = If(arraySettings(1) = "", "2", arraySettings(1))
                    ColumnDistanceTextBoxDA.Text = If(arraySettings(2) = "", "20", arraySettings(2))
                    RowDistanceTextBoxDA.Text = If(arraySettings(3) = "", "30", arraySettings(3))
                Case Else 'frames
                    ColumnsTextBoxDA.Text = "5"
                    RowsTextBoxDA.Text = "2"
                    ColumnDistanceTextBoxDA.Text = userRow.GetString("Width").ToDouble + 50
                    RowDistanceTextBoxDA.Text = userRow.GetString("Height").ToDouble + 50
            End Select
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    'private eventHandlers

    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks on the picture.
    ''' <para>It opens the <see cref="DesignCenter"/> to allow the user to select another item.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub PictureClickEventHandler(sender As Object, e As EventArgs)
        Try
            Dim latestSession = "{0};;".Compose(Registerizer.UserSetting("DCsession{0}".Compose(_command))).Cut
            Dim dialog = New DesignCenter(latestSession, _insertScale)
            If NotNothing(dialog.GetSession) Then Registerizer.UserSetting("DCsession{0}".Compose(_command), Join(dialog.GetSession, ";"))
            If Not dialog.DialogResult = DialogResult.OK Then Exit Sub

            Dim userRow = _userData.Rows(_selectedIndex)
            Dim iniDictionary = dialog.GetDescriptions.ToIniDictionary
            iniDictionary.ToList.ForEach(Sub(pair) If _userData.Columns.Contains(pair.Key) Then userRow.SetString(pair.Key, pair.Value))
            userRow.SetString("SourceFile", dialog.GetSourceFile)
            Select Case _command
                Case "DA", "DF" : Registerizer.UserData("Frames-DataTable", _userData)
                Case "DD" : Registerizer.UserData("Detectors-DataTable", _userData)
                Case "DW" : Registerizer.UserData("Wallmounts-DataTable", _userData)
            End Select
            LoadSelectedItem(_selectedIndex)
            _insertScale = dialog.GetInsertScale
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    'private subs

    ''' <summary>
    ''' Shows the itemdata and loads the respective image.
    ''' </summary>
    ''' <param name="index"></param>
    Private Sub LoadSelectedItem(index As Integer)
        If index < 0 Then Exit Sub

        Dim userRow = _userData.Rows(index)
        _sourceFile = userRow.GetString("SourceFile")
        _blockName = userRow.GetString("BlockName")
        Dim description = userRow.GetTranslation
        Dim bitmap = XRecordImagesHelper.Load("{Resources}\{0}".Compose(_sourceFile), "{Company}".Compose, "BlockPics", _blockName, Drawing.Color.Black)
        Dim pictureBox = DirectCast(Me.Controls("PictureBox"), PictureBox)
        Select Case _command
            Case "DD"
                description &= "{2L}{0}{L}{1}".Compose(userRow.GetString("Name").Translate, userRow.GetString("Text").Translate)
            Case "DW"
                _rotation = Val(userRow.GetString("Rotation"))
                Select Case _rotation
                    Case 90 : bitmap.RotateFlip(Drawing.RotateFlipType.Rotate270FlipNone)
                    Case 180 : bitmap.RotateFlip(Drawing.RotateFlipType.Rotate180FlipNone)
                    Case 270 : bitmap.RotateFlip(Drawing.RotateFlipType.Rotate90FlipNone)
                End Select
        End Select
        pictureBox.Image = bitmap
        _toolTip.SetToolTip(pictureBox, description)
        DirectCast(Me.Controls("Label"), Label).Text = _blockName
    End Sub

    ''' <summary>
    ''' Adds the picturebox and its label to the form.
    ''' </summary>
    Private Sub AddPictureBox()
        Dim pictureBox = New PictureBox
        Me.Controls.Add(pictureBox)
        Dim label = New Label
        Me.Controls.Add(label)
        With pictureBox
            .Name = "PictureBox"
            .SizeMode = PictureBoxSizeMode.CenterImage
            .Left = 187
            .Top = 12
            .Width = 84
            .Height = 60
            .BackColor = Drawing.Color.FromArgb(15, 15, 15)
            AddHandler .Click, AddressOf PictureClickEventHandler
        End With
        With label
            .Name = "Label"
            .AutoSize = False
            .TextAlign = Drawing.ContentAlignment.BottomCenter
            .Left = 187
            .Top = 9
            .Width = 84
            .Height = 78
            .BackColor = Drawing.Color.FromArgb(240, 160, 0)
            AddHandler .Click, AddressOf PictureClickEventHandler
        End With
    End Sub

    ''' <summary>
    ''' Adds the items to the itemslist.
    ''' </summary>
    Private Sub AddItemsToListBox()
        ItemsListBox.Items.Clear()
        If IsNothing(_userData) Then Exit Sub

        If _command = "DW" Then
            _userData.Select.ToList.ForEach(Sub(row) ItemsListBox.Items.Add(row.GetTranslation))
        Else
            _userData.Select.ToList.ForEach(Sub(row) ItemsListBox.Items.Add(row.GetString("Name").Translate))
        End If

        _selectedIndex = Registerizer.UserSetting("DDselected{0}".Compose(_command)).ToInteger
        If ItemsListBox.Items.Count = 0 Then Exit Sub

        Select Case ItemsListBox.Items.Count > _selectedIndex
            Case True : ItemsListBox.SetSelected(_selectedIndex, True)
            Case Else : ItemsListBox.SetSelected(0, True)
        End Select
    End Sub

End Class