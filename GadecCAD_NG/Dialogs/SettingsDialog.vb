'Gadec Engineerings Software (c) 2022
Imports System.Data
Imports System.Windows.Forms

''' <summary>
''' <para><see cref="SettingsDialog"/> allows the user to reset some settings to factory defaults.</para>
''' </summary>
Public Class SettingsDialog
    ''' <summary>
    ''' Contains all (selectable) factory settings.
    ''' </summary>
    Private ReadOnly _settingsData As DataTable
    ''' <summary>
    ''' Contains the type of setting.
    ''' </summary>
    Private ReadOnly _type As String

    'form

    ''' <summary>
    ''' Initializes a new instance of <see cref="SettingsDialog"/> with the specified data.
    ''' <para><see cref="SettingsDialog"/> allows the user to reset some settings to factory defaults.</para>
    ''' </summary>
    ''' <param name="settingsData">All (selectable) factory settings.</param>
    ''' <param name="type">Optional the type of setting.</param>
    Sub New(settingsData As DataTable, Optional type As String = "")
        ' This call is required by the designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.

        Me.Text = Registerizer.GetApplicationVersion()
        Translator.TranslateControls(Me)

        _settingsData = settingsData
        _type = type
        WireUpDialog()

        Me.ShowDialog()
    End Sub

    'buttons

    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks the OK button.
    ''' <para>It copies the selected factory defaults to the registry, sets the buttonvalue and closes the dialogbox.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub AcceptButton_Click(sender As Object, e As EventArgs) Handles ltOK.Click
        Try
            ResettingFactoryDefaults()
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

    'private subs

    ''' <summary>
    ''' Wires up the dialogbox.
    ''' </summary>
    Sub WireUpDialog()
        _settingsData.AssignPrimaryKey("Page")
        Dim row = 0
        Dim top = 0
        Dim types = _settingsData.GetUniqueStringsFromColumn("Type")
        For Each type In types
            Dim choices = _settingsData.Select("Type='{0}'".Compose(type), "Page")
            Dim choiceNames = choices.CopyToDataTable.GetStringsFromColumn(Translator.Selected)
            Dim previousChoice = _settingsData.Rows.Find(Registerizer.UserSetting("{0}-SelectedPage".Compose(type)))
            If IsNothing(previousChoice) Then previousChoice = choices.FirstOrDefault
            Dim previousString = previousChoice.GetString(Translator.Selected)

            Dim checked = _type = "" Or _type = type
            top = row * 24 + 36
            AddCheckBox(top, "CheckBox_{0}".Compose(type), checked)
            AddComboBox(top, "ComboBox_{0}".Compose(type), choiceNames, previousString)
            row += 1
        Next
        Me.Height = top + 108
        ltOK.Top = top + 36
        ltCancel.Top = top + 36
    End Sub

    ''' <summary>
    ''' Adds a checkbox to the dialogbox.
    ''' </summary>
    ''' <param name="top">Top value of the checkbox.</param>
    ''' <param name="name">The name of the checkbox</param>
    ''' <param name="checked">Determines whether the checkbox should be checked.</param>
    Private Sub AddCheckBox(top As Integer, name As String, checked As Boolean)
        Dim checkBox = New CheckBox With {
                .Name = name,
                .Tag = name,
                .Width = 14,
                .Height = 14,
                .Top = top,
                .Left = 12,
                .Checked = checked
            }
        Me.Controls.Add(checkBox)
    End Sub

    ''' <summary>
    ''' Adds a combobox to the dialogbox.
    ''' </summary>
    ''' <param name="top">Top value of the checkbox.</param>
    ''' <param name="name">The name of the checkbox</param>
    ''' <param name="items">A list of items for the combobox.</param>
    ''' <param name="previous">The previous choice.</param>
    Private Sub AddComboBox(top As Integer, name As String, items As String(), previous As String)
        Dim comboBox = New ComboBox With {
                .Name = name,
                .Tag = name,
                .Width = 330,
                .Height = 14,
                .Top = top,
                .Left = 36,
                .DropDownStyle = ComboBoxStyle.DropDownList
            }
        comboBox.Items.AddRange(items)
        comboBox.SelectedItem = previous
        Me.Controls.Add(comboBox)
    End Sub

    ''' <summary>
    ''' Copies the selected factory defaults to the registry.
    ''' </summary>
    Private Sub ResettingFactoryDefaults()
        _settingsData.AssignPrimaryKey(Translator.Selected)

        Dim types = _settingsData.GetUniqueStringsFromColumn("Type")
        For Each type In types
            Dim checkBox = DirectCast(Me.Controls("CheckBox_{0}".Compose(type)), CheckBox)
            Dim comboBox = DirectCast(Me.Controls("ComboBox_{0}".Compose(type)), ComboBox)
            If Not checkBox.Checked Then Continue For

            Dim selected = _settingsData.Rows.Find(comboBox.SelectedItem)
            If IsNothing(selected) Then Continue For

            Registerizer.UserSetting("{0}-SelectedPage".Compose(type), selected.GetString("Page"))
            Dim pageData = _settingsData.GetTable("Blocks").Select("Page='{0}'".Compose(selected.GetString("Page"))).CopyToDataTable
            Dim parameterData = DataSetHelper.LoadFromXml("{Support}\SetStandards.xml".Compose).GetTable(type, "Name")
            pageData.AssignPrimaryKey("Name")
            pageData.Merge(parameterData)
            Registerizer.UserData("{0}-DataTable".Compose(type), pageData)
        Next
    End Sub

End Class