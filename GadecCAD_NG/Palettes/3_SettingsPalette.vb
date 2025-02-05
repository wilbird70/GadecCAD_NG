'Gadec Engineerings Software (c) 2022
Imports System.Data
Imports System.Windows.Forms
Imports Autodesk.AutoCAD.Windows
Imports GadecCAD_NG.Extensions

''' <summary>
''' <para><see cref="SettingsPalette"/> provides the design and functionality for the settings tab of the <see cref="PaletteSet"/>.</para>
''' </summary>
Public Class SettingsPalette
    ''' <summary>
    ''' Determines if is the layer selection is busy, so no change of layer (with listbox) is accepted.
    ''' <para>Note: Especially important when loading the palette.</para>
    ''' </summary>
    Private _layerSelectionIsBusy As Boolean = False
    ''' <summary>
    ''' Determines if is being translated, so no change of language (with dropdownbox) is accepted.
    ''' <para>Note: Especially important when loading the palette.</para>
    ''' </summary>
    Private _translationIsBusy As Boolean = True

    'form

    ''' <summary>
    ''' Initializes a new instance of <see cref="SettingsPalette"/>.
    ''' <para><see cref="SettingsPalette"/> provides the design and functionality for the settings tab of the <see cref="PaletteSet"/>.</para>
    ''' </summary>
    Sub New()
        ' This call is required by the designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.

        AddHandler Translator.LanguageChangedEvent, AddressOf LanguageChangedEventHandler
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when user clicks on an empty part of this palette.
    ''' <para>If screenshots are enabled (by doubleclicking on the languageflag, see <see cref="SettingsPalette"/>) a screenshot of this palette will be taken.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Me_Click(sender As Object, e As EventArgs) Handles Me.Click
        Try
            If ScreenShot Then ImageHelper.GetScreenShot(PaletteHelper.GetLocation, PaletteHelper.GetSize)
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when user doubleclicks on an empty part of this palette.
    ''' <para>Throws an exception to test the exceptionsdialogbox.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Me_DoubleClick(sender As Object, e As EventArgs) Handles Me.DoubleClick
        Try
            ExceptionTest("Going wrong")
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when this palette is resizing.
    ''' <para>It handles the size and position of controls it contains.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Me_SizeChanged(sender As Object, e As EventArgs) Handles Me.SizeChanged
        Try
            If Me.Height = 0 Then Exit Sub
            Dim widthDifference = Me.Width - (ltFeedback.Left + ltFeedback.Width + 8)
            If ltFeedback.Width + widthDifference < 25 Then widthDifference = 25 - ltFeedback.Width
            LanguageComboBox.Width += widthDifference
            DepartmentsComboBox.Width += widthDifference
            LinkToPlotFolderLabel.Left += widthDifference
            LanguagePictureBox.Left += widthDifference
            PreviewCheckBox.Left += widthDifference
            OpenPdfCheckBox.Left += widthDifference
            ltReloadMenu.Width += widthDifference
            ltChangelog.Width += widthDifference
            ltManual.Width += widthDifference
            ltFeedback.Width += widthDifference
            LayersListBox.Width += (widthDifference / 2)
            LayersListBox.Left += (widthDifference / 2)
            DisciplinesListBox.Width += (widthDifference / 2)
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    'subs

    ''' <summary>
    ''' Selects the current layer in the list of typed layers in the listbox, if found.
    ''' </summary>
    Sub SelectedLayer()
        Dim doc = ActiveDocument()
        _layerSelectionIsBusy = True
        For i = 0 To LayersListBox.Items.Count - 1
            LayersListBox.SetSelected(i, False)
        Next
        Dim menuData = DataSetHelper.LoadFromXml("{Support}\SetContextMenus.xml".Compose)
        If NotNothing(doc) Then
            Dim index = LayerHelper.GetCurrentLayerTypeIndex()
            If Not index < 0 Then LayersListBox.SetSelected(index, True)
            Dim contextMenuStrip = MenuStripHelper.Create(menuData.GetTable("Display"), AddressOf ContextMenuStripClickEventHandler, My.Resources.ResourceManager)
            LayersListBox.ContextMenuStrip = contextMenuStrip
            cLayer.Text = "Current: ".Translate(SysVarHandler.GetVar("CLAYER"))
            AddHandler contextMenuStrip.VisibleChanged, AddressOf ContextMenuStripVisibleChangedEventHandler
        Else
            LayersListBox.ContextMenuStrip = Nothing
            cLayer.Text = "Current: ".Translate("")
        End If
        _layerSelectionIsBusy = False
    End Sub

    'buttons

    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks the ReloadMenu button.
    ''' <para>It reload the Gadec menu.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ReloadMenuButton_Click(sender As Object, e As EventArgs) Handles ltReloadMenu.Click
        Try
            ActiveDocument.SendString("RELOADGADEC ")
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks the Changelog button.
    ''' <para>It shows a version history messagebox.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ChangelogButton_Click(sender As Object, e As EventArgs) Handles ltChangelog.Click
        Try
            MessageBoxHistory()
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks the Manual button.
    ''' <para>It will open a pdf-file with the manual in the currently set language.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ManualButton_Click(sender As Object, e As EventArgs) Handles ltManual.Click
        Try
            ProcessHelper.StartDocument("{Support}\Gadec AutoCAD App Manual {0}.pdf".Compose(Translator.Selected))
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks the Feedback button.
    ''' <para>It will open an email with the emailaddress and subject set to send feedback.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub FeedbackButton_Click(sender As Object, e As EventArgs) Handles ltFeedback.Click
        Try
            ProcessHelper.StartMailMessage("gadec.engineerings.software@outlook.com", "Feedback {0}".Compose(Registerizer.MainSetting("mod0")), "")
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    'checkboxes

    ''' <summary>
    ''' EventHandler for the event that occurs when the user checks the Preview checkbox.
    ''' <para>It will turn on/off the image balloons.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub PreviewCheckBox_CheckedChanged(sender As Object, e As EventArgs) Handles PreviewCheckBox.CheckedChanged
        Try
            Select Case PreviewCheckBox.Checked
                Case True : Registerizer.UserSetting("HidePreviews", "True")
                Case Else : Registerizer.UserSetting("HidePreviews", "False")
            End Select
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    Private Sub OpenPdfCheckBox_CheckedChanged(sender As Object, e As EventArgs) Handles OpenPdfCheckBox.CheckedChanged
        Try
            Select Case OpenPdfCheckBox.Checked
                Case True : Registerizer.UserSetting("OpenPdf", "True")
                Case Else : Registerizer.UserSetting("OpenPdf", "False")
            End Select
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    'layers section

    ''' <summary>
    ''' EventHandler for the event that occurs when the user changes the selected discipline.
    ''' <para>It will change the layerlist for the selected discipline (also each discipline has its own color).</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub DisciplinesListBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles DisciplinesListBox.SelectedIndexChanged
        Try
            'vastleggen geselecteerde discipline
            Dim index = DirectCast(sender, ListBox).SelectedIndex
            'selecteren van lagencollectie
            Dim layerData = DataSetHelper.LoadFromXml("{Support}\SetStandards.xml".Compose).GetTable("Layers", "Name;Type")
            Dim disciplines = layerData.DefaultView.ToTable(True, "Name").GetStringsFromColumn("Name")
            Registerizer.UserSetting("Discipline", disciplines(index))
            Dim rows = layerData.Select("Name='{0}'".Compose(disciplines(index)), "Index")
            Dim layerTypes = rows.Select(Function(x) x("Type")).Cast(Of String)().ToArray
            'vullen van laagtypen tabel
            LayersListBox.Items.Clear()
            For i = 0 To layerTypes.Count - 1
                LayersListBox.Items.Add(layerTypes(i).Translate)
            Next
            'geef lagenlijst een kleurtje
            Dim colors = {
                Drawing.Color.FromArgb(235, 255, 235), 'groen
                Drawing.Color.FromArgb(255, 235, 235), 'rood
                Drawing.Color.FromArgb(255, 255, 235), 'geel
                Drawing.Color.FromArgb(235, 255, 255), 'blauw
                Drawing.Color.FromArgb(235, 235, 235)  'grijs
            }
            LayersListBox.BackColor = colors(index)
            SelectedLayer()
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user changes the selected layertype.
    ''' <para>It selects the layer to be current and if it doesn't exist it will be created first.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub LayersListBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles LayersListBox.SelectedIndexChanged
        Try
            Dim doc = ActiveDocument()
            If IsNothing(doc) Then Exit Sub
            Dim db = doc.Database
            If DocumentsHelper.GetDocumentNames.Count > 0 And Not _layerSelectionIsBusy Then
                If NotNothing(LayersListBox.SelectedItem) Then
                    Using doc.LockDocument
                        Dim layerData = DataSetHelper.LoadFromXml("{Support}\SetStandards.xml".Compose).GetTable("Layers", "Name;Type")
                        Dim layerRows = layerData.Select("Name='{0}'".Compose(Registerizer.UserSetting("Discipline")), "Index")
                        Dim layerTypes = layerRows.Select(Function(x) x("Type")).Cast(Of String)().ToArray
                        Dim layerId = LayerHelper.GetLayerIdFromType(db, layerTypes(LayersListBox.SelectedIndex))
                        Dim layerName = New LayerHandler(db).SelectById(layerId)
                        cLayer.Text = "Current: ".Translate(layerName)
                    End Using
                End If
            End If
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    'language section

    ''' <summary>
    ''' EventHandler for the event that occurs when the user changes the selected language.
    ''' <para>It sets the new language and raises the LanguageChanged event.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub LanguageComboBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles LanguageComboBox.SelectedIndexChanged
        Try
            If _translationIsBusy Then Exit Sub

            Translator.SetLanguange(LanguageComboBox.SelectedIndex)
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user doubleclicks the languageflag.
    ''' <para>It toggels the screenshot mode.</para>
    ''' </summary>
    Private Sub LanguagePictureBox_DoubleClick(sender As Object, e As EventArgs) Handles LanguagePictureBox.DoubleClick
        Try
            ScreenShot = Not ScreenShot
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    'plotters section

    ''' <summary>
    ''' EventHandler for the event that occurs when the user changes the selected department.
    ''' <para>It will change the plotstyles and the active plotter/printer.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub DepartmentsComboBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles DepartmentsComboBox.SelectedIndexChanged
        Try
            Dim departmentData = DataSetHelper.LoadFromXml("{Support}\SetPlotSettings.xml".Compose).GetTable("Departments", "Name")
            Dim departmentKeys = departmentData.GetStringsFromColumn("Name")
            Registerizer.UserSetting("Department", departmentKeys(DepartmentsComboBox.SelectedIndex))
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks the LinkToPlotFolder label.
    ''' <para>It will opens the AutoCAD plotfolder in explorer.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub LinkToPlotFolderLabel_Click(sender As Object, e As EventArgs) Handles LinkToPlotFolderLabel.Click
        Try
            Dim preferences As Autodesk.AutoCAD.Interop.AcadPreferences
            preferences = Autodesk.AutoCAD.ApplicationServices.Application.Preferences
            ProcessHelper.StartExplorer(preferences.Files.PrinterConfigPath)
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    'eventHandlers

    ''' <summary>
    ''' EventHandler for the event that occurs when the language has changed.
    ''' <para>It translates this palette.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub LanguageChangedEventHandler(sender As Object, e As LanguageChangedEventArgs)
        'image
        Dim flagImageFileName = "{Support}\Lang_{0}.png".Compose(e.Selected)
        LanguagePictureBox.Image = Drawing.Image.FromFile(flagImageFileName)
        'wireup combobox
        _translationIsBusy = True
        Dim languageKeys = e.AvialableLanguages
        If languageKeys.Count > 0 Then
            Dim languageIndex = 0
            LanguageComboBox.Items.Clear()
            For Each k In languageKeys
                LanguageComboBox.Items.Add(("Lang_{0}".Compose(k)).Translate)
                If k = e.Selected Then languageIndex = LanguageComboBox.Items.Count - 1
            Next
            If LanguageComboBox.Items.Count > 0 Then LanguageComboBox.SelectedIndex = languageIndex
        End If
        _translationIsBusy = False
        'tekst voor knoppen en labels
        Translator.TranslateControls(Me)
        'lijst met disciplines
        Dim layerData = DataSetHelper.LoadFromXml("{Support}\SetStandards.xml".Compose).GetTable("Layers", "Name;Type")
        Dim disciplines = layerData.DefaultView.ToTable(True, "Name").GetStringsFromColumn("Name")
        Dim currentDiscipline = Registerizer.UserSetting("Discipline")
        DisciplinesListBox.Items.Clear()
        Dim count1 = 0
        For i = 0 To disciplines.Count - 1
            DisciplinesListBox.Items.Add(disciplines(i).Translate)
            If disciplines(i) = currentDiscipline Then count1 = i
        Next
        DisciplinesListBox.SetSelected(count1, True)
        'lijst met departments
        Dim departmentData = DataSetHelper.LoadFromXml("{Support}\SetPlotSettings.xml".Compose).GetTable("Departments", "Name")
        Dim dpmKeys = departmentData.GetStringsFromColumn("Name")
        Dim currentDepartment = Registerizer.UserSetting("Department")
        DepartmentsComboBox.Items.Clear()
        Dim count2 = 0
        For i = 0 To dpmKeys.Count - 1
            DepartmentsComboBox.Items.Add(dpmKeys(i))
            If dpmKeys(i) = currentDepartment Then count2 = i
        Next
        DepartmentsComboBox.SelectedIndex = count2
        OpenPdfCheckBox.Checked = (Registerizer.UserSetting("OpenPdf") = "True")
        PreviewCheckBox.Checked = (Registerizer.UserSetting("HidePreviews") = "True")
        SelectedLayer()
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user selects an option in the contextmenu.
    ''' <para>It will executes the selected option.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ContextMenuStripClickEventHandler(sender As Object, e As EventArgs)
        Try
            Dim doc = ActiveDocument()
            doc.CancelCommand
            'tag van geselecteerde optie
            Dim tag = DirectCast(sender, ToolStripItem).Tag.ToString
            'bepalen geselecteerde layer
            Dim layer = ""
            If LayersListBox.SelectedIndex >= 0 Then
                Dim layerData = DataSetHelper.LoadFromXml("{Support}\SetStandards.xml".Compose).GetTable("Layers", "Name;Type")
                Dim rows = layerData.Select("Name='{0}'".Compose(Registerizer.UserSetting("Discipline")), "Index")
                Dim layerTypes = rows.Select(Function(x) x("Type")).Cast(Of String)().ToArray
                Dim row = layerData.Rows.Find({Registerizer.UserSetting("Discipline"), layerTypes(LayersListBox.SelectedIndex)})
                layer = row.GetString("LName")
            End If
            'functie uitvoeren afhankelijk van geselecteerde optie
            Select Case True
                Case LayersListBox.SelectedIndex < 0
                Case tag = "ToTop" : DrawOrderHelper.BringToFront(doc, layer)
                Case tag = "ToBottom" : DrawOrderHelper.SendToBack(doc, layer)
            End Select
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the visibility of the contextmenu changes.
    ''' <para>If screenshots are enabled (by doubleclicking on the languageflag, see <see cref="SettingsPalette"/>) a screenshot of the contextmenu will be taken.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ContextMenuStripVisibleChangedEventHandler(sender As Object, e As EventArgs)
        Try
            If ScreenShot Then
                Dim toolStrip = DirectCast(sender, ToolStrip)
                If toolStrip.Visible Then ImageHelper.GetScreenShot(toolStrip.Location, toolStrip.Size)
            End If
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    'private subs

    ''' <summary>
    ''' Throws an exception to test the exceptionsdialogbox.
    ''' </summary>
    ''' <param name="text"></param>
    ''' <returns></returns>
    Private Function ExceptionTest(text As String) As Boolean
        Try
            Dim dictionary = New Dictionary(Of String, String)
            Dim test = dictionary(text)
            Return True
        Catch ex As Exception
            ex.AddData("ExceptionTest")
            ex.Rethrow
            Return False
        End Try
    End Function

End Class
