'Gadec Engineerings Software (c) 2022
Imports System.Windows.Forms

''' <summary>
''' <para><see cref="ListBoxDialog"/> allows the user to select an option from a list.</para>
''' </summary>
Public Class ListBoxDialog

    'form

    ''' <summary>
    ''' Initializes a new instance of <see cref="ListBoxDialog"/>.
    ''' <para><see cref="ListBoxDialog"/> allows the user to select an option from a list.</para>
    ''' </summary>
    ''' <param name="prompt">Prompt message in the dialog.</param>
    ''' <param name="items">A list of items to select from.</param>
    ''' <param name="preSelect">One of the items to preselect.</param>
    ''' <param name="checkboxText">Text to display with an optional checkbox.</param>
    Sub New(prompt As String, items As String(), Optional preSelect As String = "", Optional checkboxText As String = "")
        ' This call is required by the designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.

        Me.Text = Registerizer.GetApplicationVersion()
        Translator.TranslateControls(Me)

        lDescr.Text = prompt
        Dim preSel = 0
        For i = 0 To items.Count - 1
            If items(i) = preSelect Then preSel = i
            InputListBox.Items.Add(items(i).Compose)
        Next
        InputListBox.SetSelected(preSel, True)
        If Not checkboxText = "" Then
            InputCheckBox.Text = checkboxText
            InputCheckBox.Visible = True
        End If
        Me.Height = (InputListBox.ItemHeight * InputListBox.Items.Count) + 128
        Me.ShowDialog()
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the dialogbox gets the focus.
    ''' <para>It sets the focus to the listbox.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Me_Activated(sender As Object, e As EventArgs) Handles Me.Activated
        Try
            InputListBox.Focus()
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
            If Me.Height < 158 Then Me.Height = 158
            ltOK.Top = Me.Height - 73
            ltCancel.Top = Me.Height - 73
            InputCheckBox.Top = Me.Height - 73
            InputListBox.Height = Me.Height - 104

            If Me.Width < 300 Then Me.Width = 300
            ltOK.Left = Me.Width - 205
            ltCancel.Left = Me.Width - 113
            InputListBox.Width = Me.Width - 40
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    'functions

    ''' <summary>
    ''' Gets the checked value of the optional checkbox.
    ''' </summary>
    ''' <returns>The checked value.</returns>
    Function GetCheckState() As Boolean
        Return InputCheckBox.Checked
    End Function

    ''' <summary>
    ''' Gets the index of the selected item.
    ''' </summary>
    ''' <returns>The index.</returns>
    Function GetSelectedIndex() As Integer
        Return InputListBox.SelectedIndex
    End Function

    'buttons

    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks the OK button or doubleclicks en item.
    ''' <para>It sets the buttonvalue and closes the dialogbox.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub AcceptButton_Click(sender As Object, e As EventArgs) Handles ltOK.Click, InputListBox.DoubleClick
        Try
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
            Me.Hide()
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

End Class