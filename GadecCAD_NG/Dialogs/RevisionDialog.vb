'Gadec Engineerings Software (c) 2022
Imports System.Windows.Forms

''' <summary>
''' <para><see cref="RevisionDialog"/> allows the user to enter data for a new revision.</para>
''' </summary>
Public Class RevisionDialog

    'form

    ''' <summary>
    ''' Initializes a new instance of <see cref="RevisionDialog"/> with the specified data.
    ''' <para><see cref="RevisionDialog"/> allows the user to enter data for a new revision.</para>
    ''' </summary>
    ''' <param name="drawnIni">The initials of the designer.</param>
    ''' <param name="checkIni">The initials of the controller.</param>
    Sub New(drawnIni As String, checkIni As String)
        ' This call is required by the designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.

        Me.Text = Registerizer.GetApplicationVersion()
        Translator.TranslateControls(Me)

        InputDateTimePicker.Value = Now
        DescriptionComboBox.Items.AddRange("REVTEXTS".Translate.Cut)
        DrawnTextBox.Text = drawnIni
        CheckTextBox.Text = checkIni
        DescriptionComboBox.Select()
        Me.ShowDialog()
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
    ''' Gets the (users input) revision texts.
    ''' </summary>
    ''' <returns></returns>
    Function GetRevisionTexts() As Dictionary(Of String, String)
        Dim output = New Dictionary(Of String, String) From {
            {"Date", InputDateTimePicker.Value.ToString("dd-MM-yyyy")},
            {"Descr", DescriptionComboBox.Text},
            {"Drawn", DrawnTextBox.Text},
            {"Check", CheckTextBox.Text},
            {"Char", ""},
            {"KOPREV", ""}
        }
        Return output
    End Function

    ''' <summary>
    ''' Gets the (users input) revision texts.
    ''' </summary>
    ''' <returns></returns>
    Function GetRevisionData() As String()
        Dim iniString = "Date={0};Descr={1};Drawn={2};Check={3};Char=;KOPREV="
        Dim output = iniString.Compose(InputDateTimePicker.Value.ToString("dd-MM-yyyy"), DescriptionComboBox.Text, DrawnTextBox.Text, CheckTextBox.Text)
        Return output.Cut
    End Function

    'buttons

    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks the OK button.
    ''' <para>It sets the buttonvalue and closes the dialogbox.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub AcceptButton_Click(sender As Object, e As EventArgs) Handles ltOK.Click
        Try
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