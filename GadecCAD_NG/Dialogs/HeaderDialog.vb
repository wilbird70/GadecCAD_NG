'Gadec Engineerings Software (c) 2022
Imports System.Data
Imports System.Windows.Forms

''' <summary>
''' <para><see cref="HeaderDialog"/> allows the user to adjust the data in the header of the selected frame(s).</para>
''' </summary>
Public Class HeaderDialog
    ''' <summary>
    ''' Contains the string to display if the data in the headers is different.
    ''' </summary>
    Private ReadOnly _varies As String
    ''' <summary>
    ''' A record containing the combined data of the headers.
    ''' </summary>
    Private ReadOnly _header As DataRow

    ''' <summary>
    ''' Determines whether the user initials have been modified.
    ''' </summary>
    Private _initialsChanged As Boolean = False
    ''' <summary>
    ''' Contains the data of a header, selected in the overviewdialog.
    ''' </summary>
    Private _copied As DataRow
    ''' <summary>
    ''' Contains the dialogdata before opening the overviewdialog.
    ''' </summary>
    Private _previous As DataRow

    'form

    ''' <summary>
    ''' Initializes a new instance of <see cref="HeaderDialog"/> with the specified data.
    ''' <para><see cref="HeaderDialog"/> allows the user to adjust the data in the header of the selected frame(s).</para>
    ''' </summary>
    ''' <param name="header">A record containing the combined data of the headers.</param>
    ''' <param name="saved">Determines whether the active document is saved or not.</param>
    ''' <param name="varies">The string to display if the data in the headers is different.</param>
    Sub New(header As DataRow, saved As Boolean, varies As String)
        ' This call is required by the designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.

        Me.Text = Registerizer.GetApplicationVersion()
        Translator.TranslateControls(Me)

        _header = header
        _varies = varies

        tRev.AutoSize = False
        tRev.Height = 59
        Dim bitmap = New Drawing.Bitmap(GadecPictureBox.Image)
        bitmap.MakeTransparent(Drawing.Color.White)
        GadecPictureBox.Image = bitmap

        Dim dateBecomeToday = False
        For Each control In Me.Controls.ToArray
            If Not control.Name.StartsWith("t") Then Continue For

            If Not _header.HasValue(control.Name.EraseStart(1)) Then control.Enabled = False : Continue For

            Dim controlName = control.Name.EraseStart(1)
            control.Text = _header.GetString(controlName)
            If control.Text = _varies Then control.BackColor = Drawing.Color.LightSteelBlue
            Select Case True
                Case controlName = "Descr"
                    If control.Text = "" Then control.Text = "DESIGNED".Translate
                Case controlName = "Date"
                    If control.Text = "" Then control.Text = Format(Now, "dd-MM-yyyy") : dateBecomeToday = True
                Case controlName = "Drawn"
                    If control.Text = "" Then control.Text = Registerizer.UserSetting("UserIni")
                Case Not (controlName = "Size" Or controlName = "Scale")
                Case control.Text.StartsWith("$*")
                    control.Enabled = True
                    control.Text = _header.GetString(controlName).EraseStart(2)
            End Select
            AddHandler control.Enter, AddressOf TextBoxEnterEventHandler
        Next
        If Not saved Then OverviewButton.Enabled = False
        Me.ShowDialog()
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when this dialogbox is activated.
    ''' <para>It sets the focus on the OK button.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Me_Activated(sender As Object, e As EventArgs) Handles Me.Activated
        Try
            ltOK.Select()
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
    ''' Gets a record containing the adjusted headerdata.
    ''' </summary>
    ''' <returns>The adjusted headerdata.</returns>
    Function GetHeader() As DataRow
        Return _header
    End Function

    'buttons

    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks the OK button.
    ''' <para>It stores the adjusted headerdata, sets the buttonvalue and closes the dialogbox.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub AcceptButton_Click(sender As Object, e As EventArgs) Handles ltOK.Click
        Try
            If _initialsChanged Then Registerizer.UserSetting("UserIni", tDrawn.Text)
            For Each control In Me.Controls.ToArray
                If control.Name.StartsWith("t") Then
                    Dim controlName = control.Name.EraseStart(1)
                    If _header.HasValue(controlName) Then _header(controlName) = control.Text
                End If
            Next
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
            Me.Hide()
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks the GoogleSeach button.
    ''' <para>It takes the clientdata (up to four fields) to search for with google.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub GoogleSeachButton_Click(sender As Object, e As EventArgs) Handles GoogleSeachButton.Click
        Try
            Dim url = "www.google.nl/search?hl=nl&q={0}+{1}+{2}+{3}&btnG=Google+zoeken&meta=&gws_rd=ssl".Compose(tClient1.Text, tClient2.Text, tClient3.Text, tClient4.Text).Replace(" ", "+")
            ProcessHelper.StartBrowser(url)
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks the Overview button.
    ''' <para>It shows the <see cref="OverviewDialog"/> where the user can select a frame to copy data from.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub OverviewButton_Click(sender As Object, e As EventArgs) Handles OverviewButton.Click
        Try
            Dim dataBuilder = New DataBuilder("Previous")
            For Each control In Me.Controls.ToArray
                If control.Name.StartsWith("t") Then dataBuilder.AppendValue("_{0}".Compose(control.Name), control.Text)
            Next
            dataBuilder.AddNewlyCreatedRow()
            _previous = dataBuilder.GetDataTable.Rows(0)
            Dim dialog = New OverviewDialog(True)
            If Not dialog.DialogResult = DialogResult.OK Then Exit Sub

            _copied = dialog.GetSelectedRow
            EnteringPasteMode()
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    'textboxes

    ''' <summary>
    ''' EventHandler for the event that occurs when the user changes the initials of the drawer.
    ''' <para>It sets a value to true that causes the modified initials to be saved in the registry.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub DrawnTextBox_KeyPress(sender As Object, e As KeyPressEventArgs) Handles tDrawn.KeyPress
        _initialsChanged = True
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the text in the revision textbox is changed.
    ''' <para>It changed the size of the font to small if the value is 'varies'.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub RevisionTextBox_TextChanged(sender As Object, e As EventArgs) Handles tRev.TextChanged
        Try
            Select Case tRev.Text = _varies
                Case True : tRev.Font = New Drawing.Font("Microsoft Sans Serif", 8.25)
                Case Else : tRev.Font = New Drawing.Font("Microsoft Sans Serif", 27.75)
            End Select
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    'eventhandlers

    ''' <summary>
    ''' EventHandler for the event that occurs when the text a textbox is changed.
    ''' <para>If the value is 'varies' the whole text will be selected.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub TextBoxEnterEventHandler(sender As Object, e As EventArgs)
        Try
            Dim textBox = DirectCast(sender, TextBox)
            If textBox.Text = _varies Then
                BeginInvoke(DirectCast(Sub() textBox.SelectAll(), Action))
            End If
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the check of a checkbox is changed.
    ''' <para>It sets other checkboxes accordingly.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub CheckBoxCheckedChangedEventHandler(sender As Object, e As EventArgs)
        Try
            Dim checkBox = DirectCast(sender, CheckBox)
            Static isBusy As Boolean
            If Not isBusy Then
                isBusy = True
                Select Case checkBox.Name
                    Case "_ltDescr"
                        SetCheckBoxes(checkBox.CheckState, "_tDescr1", "_tDescr2", "_tDescr3")
                    Case "_ltClient"
                        SetCheckBoxes(checkBox.CheckState, "_tClient1", "_tClient2", "_tClient3", "_tClient4")
                    Case "_ltProject"
                        SetCheckBoxes(checkBox.CheckState, "_tProject", "_tDossier", "_tRev", "_tDrawing", "_tSheet", "_tSize", "_tScale")
                    Case "_ltDate"
                        SetCheckBoxes(checkBox.CheckState, "_tDate", "_tDescr", "_tDesign", "_tDrawn", "_tCheck")
                    Case "_tDescr1", "_tDescr2", "_tDescr3"
                        SetMainCheckBox("_ltDescr", "_tDescr1", "_tDescr2", "_tDescr3")
                    Case "_tClient1", "_tClient2", "_tClient3", "_tClient4"
                        SetMainCheckBox("_ltClient", "_tClient1", "_tClient2", "_tClient3", "_tClient4")
                    Case "_tProject", "_tDossier", "_tRev", "_tDrawing", "_tSheet", "_tSize", "_tScale"
                        SetMainCheckBox("_ltProject", "_tProject", "_tDossier", "_tRev", "_tDrawing", "_tSheet", "_tSize", "_tScale")
                    Case "_tDate", "_tDescr", "_tDesign", "_tDrawn", "_tCheck"
                        SetMainCheckBox("_ltDate", "_tDate", "_tDescr", "_tDesign", "_tDrawn", "_tCheck")
                End Select
                isBusy = False
            End If
            If checkBox.Name.StartsWith("_t") Then
                Dim textBox = Me.Controls(checkBox.Name.EraseStart(1))
                Select Case True
                    Case Not checkBox.Checked
                        textBox.Text = _previous(checkBox.Name)
                        textBox.BackColor = Drawing.SystemColors.Window
                        If textBox.Text = _varies Then textBox.BackColor = Drawing.Color.LightSteelBlue
                    Case _copied.Table.Columns.Contains(checkBox.Name.EraseStart(2))
                        textBox.Text = _copied.GetString(checkBox.Name.EraseStart(2))
                        textBox.BackColor = Drawing.Color.PeachPuff
                End Select
            End If
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    'private subs

    ''' <summary>
    ''' Sets the checkboxes with the specified names equal to the specifies value.
    ''' </summary>
    ''' <param name="value"></param>
    ''' <param name="checkBoxNames"></param>
    Private Sub SetCheckBoxes(value As CheckState, ParamArray checkBoxNames As String())
        For Each control In Me.Controls.ToArray
            If checkBoxNames.Contains(control.Name) Then DirectCast(control, CheckBox).CheckState = value
        Next
    End Sub

    ''' <summary>
    ''' Sets the main status according to the checkboxes with the specified names.
    ''' </summary>
    ''' <param name="mainCheckBoxName"></param>
    ''' <param name="checkBoxNames"></param>
    Private Sub SetMainCheckBox(mainCheckBoxName As String, ParamArray checkBoxNames As String())
        Dim checkState = -1
        Dim mainCheckBox As CheckBox = Nothing
        Dim checkBoxes = New List(Of CheckBox)
        For Each control In Me.Controls.ToArray
            Select Case True
                Case checkBoxNames.Contains(control.Name) : checkBoxes.Add(DirectCast(control, CheckBox))
                Case mainCheckBoxName = control.Name : mainCheckBox = control
            End Select
        Next
        For Each checkBox In checkBoxes
            Select Case checkState
                Case -1 : checkState = checkBox.CheckState
                Case 0 : If checkBox.CheckState = 1 Then checkState = 2
                Case 1 : If checkBox.CheckState = 0 Then checkState = 2
            End Select
        Next
        If NotNothing(mainCheckBox) And Not checkState < 0 Then mainCheckBox.CheckState = checkState
    End Sub

    ''' <summary>
    ''' Fills in the copied text if the previous value isn't 'varies' and adds checkboxes to all textboxes and sets them accordingly.
    ''' <para>Also adds the four main checkboxes.</para>
    ''' </summary>
    Private Sub EnteringPasteMode()
        Me.SuspendLayout()
        Dim controls = New List(Of Control)
        For Each control In Me.Controls.ToArray
            If control.Enabled And control.Name.StartsWith("t") Then controls.Add(control)
            Select Case control.Name
                Case "ltDescr", "ltClient", "ltProject", "ltDate" : controls.Insert(0, control)
            End Select
        Next
        For Each control In controls
            Dim checkBox = New CheckBox With {
                .Name = "_{0}".Compose(control.Name),
                .Width = 14,
                .Height = 14,
                .Top = control.Top + 3,
                .Left = control.Left - 16
            }
            Me.Controls.Add(checkBox)
            checkBox.BringToFront()
            AddHandler checkBox.CheckedChanged, AddressOf CheckBoxCheckedChangedEventHandler
            Select Case True
                Case control.Name.StartsWith("lt")
                    checkBox.Top = control.Top - 1
                    checkBox.Left = control.Left - 16
                Case control.Text = _varies
                    checkBox.Checked = False
                Case _copied.Table.Columns.Contains(control.Name.EraseStart(1))
                    control.Text = _copied.GetString(control.Name.EraseStart(1))
                    checkBox.Checked = True
                Case Else
                    checkBox.Checked = True
            End Select
        Next
        Me.ResumeLayout()
    End Sub

End Class