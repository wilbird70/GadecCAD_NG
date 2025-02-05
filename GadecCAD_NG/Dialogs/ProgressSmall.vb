'Gadec Engineerings Software (c) 2022
Imports System.Windows.Forms

''' <summary>
''' <para><see cref="ProgressSmall"/> provides only a progressbar.</para>
''' <para>This dialog is no longer used, but has not yet been removed for possible future use.</para>
''' </summary>
Public Class ProgressSmall

    'form

    ''' <summary>
    ''' Initializes a new instance of <see cref="ProgressSmall"/> with the specified properties.
    ''' <para><see cref="ProgressSmall"/> provides only a progressbar.</para>
    ''' </summary>
    ''' <param name="maximum">The number of items to process.</param>
    Sub New(maximum As Integer)
        ' This call is required by the designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.

        PromptLabel.Text = ""
        OutputProgessBar.Value = 0
        OutputProgessBar.Maximum = (maximum * 1000) + 2
        Me.Show()
    End Sub

    'subs

    ''' <summary>
    ''' Stops the progressbar and hides (disposes) the dialog.
    ''' </summary>
    Sub StopProgress()
        Me.Hide()
        Me.Dispose()
    End Sub

    ''' <summary>
    ''' Sets the progress.
    ''' </summary>
    ''' <param name="prompt">The text to display in the dialog.</param>
    ''' <param name="value">The number of items processed.</param>
    Sub SetProgress(prompt As String, value As Integer)
        Dim maximum = CInt(OutputProgessBar.Maximum / 1000)
        Dim current = CInt(OutputProgessBar.Value / 1000)
        Dim newValue = value
        Select Case True
            Case newValue > -1 : If newValue > maximum Then newValue = maximum
            Case current < maximum : newValue = current + 1
            Case Else : newValue = current : Me.Text = "Waarde te hoog" : Beep()
        End Select
        OutputProgessBar.Value = newValue * 1000
        OutputProgessBar.Value += 1
        OutputProgessBar.Value -= 1
        PromptLabel.Text = prompt
        Application.DoEvents()
        Me.Refresh()
    End Sub

End Class