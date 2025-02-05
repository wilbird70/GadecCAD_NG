'Gadec Engineerings Software (c) 2022

''' <summary>
''' <para><see cref="InputBoxDialog"/> allows the user to enter a text string. Unlike the inputbox function, the default response is fully selected.</para>
''' </summary>
Public Class InputBoxDialog
    ''' <summary>
    ''' Contains the entered text.
    ''' </summary>
    ''' <returns>The entered text</returns>
    Public Property InputText As String = ""

    'form

    ''' <summary>
    ''' Initializes a new instance of <see cref="InputBoxDialog"/>.
    ''' <para><see cref="InputBoxDialog"/> allows the user to enter a text string. Unlike the inputbox function, the default response is fully selected.</para>
    ''' </summary>
    ''' <param name="prompt">Prompt message in the dialog.</param>
    ''' <param name="defaultResponse">The default value in the textbox.</param>
    Sub New(prompt As String, defaultResponse As String)
        ' This call is required by the designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.

        Me.Text = Registerizer.GetApplicationVersion()
        Translator.TranslateControls(Me)

        lDescr.Text = prompt
        InputTextBox.Text = defaultResponse
        Me.ShowDialog()
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the dialogbox gets the focus.
    ''' <para>It selects the default value and sets the focus to the textbox.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Me_Activated(sender As Object, e As EventArgs) Handles Me.Activated
        Try
            InputTextBox.SelectionStart = 0
            InputTextBox.SelectionLength = 100
            InputTextBox.Focus()
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
            If Me.Height < 144 Then Me.Height = 144
            If Me.Width < 300 Then Me.Width = 300
            ltOK.Left = Me.Width - 205
            ltCancel.Left = Me.Width - 113
            InputTextBox.Width = Me.Width - 40
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    'buttons


    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks the OK button or doubleclicks en item.
    ''' <para>It sets the entered text to a property, sets the buttonvalue and closes the dialogbox.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub AcceptButton_Click(sender As Object, e As EventArgs) Handles ltOK.Click
        Try
            InputText = InputTextBox.Text
            Me.Hide()
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub


    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks the OK button or doubleclicks en item.
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