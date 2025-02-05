'Gadec Engineerings Software (c) 2022
Imports System.Windows.Forms

''' <summary>
''' <para><see cref="MessageBoxDialog"/> can display a message and allow the user to respond to it with one of the buttons.</para>
''' </summary>
Public Class MessageBoxDialog
    ''' <summary>
    ''' Has the number of the button that was clicked.
    ''' </summary>
    ''' <returns>The button number.</returns>
    Public ReadOnly Property ButtonNumber As Integer = -1

    ''' <summary>
    ''' The tooltip to use on this dialogbox.
    ''' </summary>
    Private ReadOnly _toolTip As New ToolTip With {.AutomaticDelay = 200}
    ''' <summary>
    ''' Contains the number of lines of the message.
    ''' </summary>
    Private ReadOnly _linesCount As Integer

    'form

    ''' <summary>
    ''' Initializes a new instance of <see cref="MessageBoxDialog"/> with the specified properties.
    ''' <para><see cref="MessageBoxDialog"/> can display a message and allow the user to respond to it with one of the buttons.</para>
    ''' </summary>
    ''' <param name="prompt">Prompt message in the dialog.</param>
    ''' <param name="texts">A list of texts to display.</param>
    ''' <param name="buttons">A dictionary with (max. 3) key (button text) value (button tooltip) pairs.</param>
    Sub New(prompt As String, texts As String(), buttons As Dictionary(Of String, String))
        ' This call is required by the designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.

        Me.Text = Registerizer.GetApplicationVersion()
        Translator.TranslateControls(Me)

        CaptionLabel.Text = prompt
        OutputTextBox.Text = String.Join(vbLf, texts)

        For i = 0 To {buttons.Count - 1, 2}.Min
            Dim button = DirectCast(Controls("Button{0}".Compose(i)), Button)
            button.Text = buttons.Keys(i)
            button.Visible = True
            _toolTip.SetToolTip(button, buttons.Values(i))
        Next
        _linesCount = texts.Count
        Me.ShowDialog()
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the dialogbox is loading.
    ''' <para>It changes the height of the dialog depending on the size of the message (number of lines).</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Me_Load(sender As Object, e As EventArgs) Handles Me.Load
        Me.Height = {(_linesCount * 13) + 130, 0.8 * ApplicationHelper.WindowSize.Height}.Min
        ltCancel.Focus()
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
            Button2.Top = Me.Height - 73
            Button1.Top = Me.Height - 73
            Button0.Top = Me.Height - 73
            ltCancel.Top = Me.Height - 73
            OutputTextBox.Height = Me.Height - 104

            If Me.Width < 395 Then Me.Width = 395
            Button0.Left = Me.Width - 383
            Button1.Left = Me.Width - 293
            Button2.Left = Me.Width - 203
            ltCancel.Left = Me.Width - 113
            OutputTextBox.Width = Me.Width - 40

        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    'buttons

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
    ''' EventHandler for the event that occurs when the user clicks the first button.
    ''' <para>It sets the buttonvalue and closes the dialogbox.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Button0_Click(sender As Object, e As EventArgs) Handles Button0.Click
        Try
            _ButtonNumber = 0
            Me.Hide()
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks the second button.
    ''' <para>It sets the buttonvalue and closes the dialogbox.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Try
            _ButtonNumber = 1
            Me.Hide()
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks the third button.
    ''' <para>It sets the buttonvalue and closes the dialogbox.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Try
            _ButtonNumber = 2
            Me.Hide()
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

End Class