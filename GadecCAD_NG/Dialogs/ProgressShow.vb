'Gadec Engineerings Software (c) 2022
Imports System.Windows.Forms

''' <summary>
''' <para><see cref="ProgressShow"/> provides a progressbar with cancel button.</para>
''' </summary>
Public Class ProgressShow
    ''' <summary>
    ''' Determines whether the cancel button is pressed.
    ''' </summary>
    ''' <returns>True if pressed.</returns>
    Public ReadOnly Property CancelPressed As Boolean = False

    ''' <summary>
    ''' Contains the number of items processed.
    ''' </summary>
    Private _value As Long = 0
    ''' <summary>
    ''' Contains a timer that ticks (30 s) to terminate the dialog when not responding.
    ''' </summary>
    Private ReadOnly _timer As Timer

    'form

    ''' <summary>
    ''' Initializes a new instance of <see cref="ProgressShow"/> with the specified properties.
    ''' <para><see cref="ProgressShow"/> provides a progressbar with cancel button.</para>
    ''' </summary>
    ''' <param name="prompt">The text to display in the dialog.</param>
    ''' <param name="maximum">The number of items to process.</param>
    ''' <param name="hideCancelButton">If true, hides the cancel button.</param>
    Sub New(prompt As String, maximum As Integer, Optional hideCancelButton As Boolean = False)
        ' This call is required by the designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.

        Me.Text = Registerizer.GetApplicationVersion()
        Translator.TranslateControls(Me)

        OutputProgressBar.Maximum = maximum * 1000
        OutputProgressBar.Minimum = 0
        OutputProgressBar.Step = 1000
        OutputProgressBar.Value = 10
        OutputProgressBar.Style = ProgressBarStyle.Continuous
        OutputProgressBar.Value -= 1
        _timer = New Timer With {.Interval = 30000}
        AddHandler _timer.Tick, AddressOf TimerTickEventHandler
        _timer.Start()
        Me.TopMost = True
        Me.Show()
        Me.Refresh()

        ValueLabel.Text = "0"
        ValueLabel.Refresh()
        MaxLabel.Text = maximum.ToString
        MaxLabel.Refresh()
        ltCancel.Visible = Not hideCancelButton

        SetText(prompt)
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
    ''' EventHandler for the event that occurs when user closes this dialog (with the X on the top right corner).
    ''' <para>It sets the cancel pressed property to true.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Me_Closed(sender As Object, e As EventArgs) Handles Me.Closed
        _CancelPressed = True
    End Sub

    'subs

    ''' <summary>
    ''' Performs a step to the progressbar.
    ''' </summary>
    ''' <param name="prompt">The text to display in the dialog.</param>
    Public Sub PerformStep(Optional prompt As String = "$")
        _timer.Stop()
        _value += 1
        ValueLabel.Text = _value.ToString
        ValueLabel.Refresh()
        OutputProgressBar.PerformStep()
        OutputProgressBar.Value -= 1
        If Not prompt = "$" Then SetText(prompt)
        Application.DoEvents()
        _timer.Start()
    End Sub

    ''' <summary>
    ''' Sets the prompt text and refreshes the prompt label.
    ''' </summary>
    ''' <param name="prompt">The text to display in the dialog.</param>
    Public Sub SetText(prompt As String)
        PromptLabel.Text = prompt
        PromptLabel.Refresh()
    End Sub

    ''' <summary>
    ''' Sets a new maximum of items to process.
    ''' </summary>
    ''' <param name="maximum">The number of items to process.</param>
    Public Sub SetMaximum(maximum As Integer)
        If OutputProgressBar.Value > maximum * 1000 Then OutputProgressBar.Value = maximum * 1000
        OutputProgressBar.Value = 1
        OutputProgressBar.Maximum = 1000 * {maximum, 1}.Max
        OutputProgressBar.Value -= 1
        MaxLabel.Text = maximum.ToString
        MaxLabel.Refresh()
    End Sub

    'buttons

    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks the Cancel button.
    ''' <para>It sets the cancel pressed property to true.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub CancelButton_Click(sender As Object, e As EventArgs) Handles ltCancel.Click
        _CancelPressed = True
    End Sub

    'private eventhandlers

    ''' <summary>
    ''' EventHandler for the event that occurs when the timer ticks.
    ''' <para>It hides (disposes) the dialog.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub TimerTickEventHandler(sender As Object, e As EventArgs)
        Try
            Me.Dispose()
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

End Class