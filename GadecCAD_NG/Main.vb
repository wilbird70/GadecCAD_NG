'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.EditorInput
Imports GadecCAD
Imports GadecCAD_NG.Extensions

<Assembly: Runtime.Versioning.SupportedOSPlatform("windows")>
Public Module Main
    ''' <summary>
    ''' A public field that contains a dialogbox with progressbar.
    ''' </summary>
    Public Property Progressbar As ProgressShow
    ''' <summary>
    ''' A public field that contains a dictionary for holding the group prefix length for each project (folder).
    ''' </summary>
    Public Property PrefixLengths As New Dictionary(Of String, Integer) From {{"", -1}}
    ''' <summary>
    ''' A public field that specifies whether to take a screenshot.
    ''' </summary>
    Public Property ScreenShot As Boolean = False

    'subs

    ''' <summary>
    ''' <para>Provides the startup for this application.</para>
    ''' </summary>
    Public Sub StartGadec()
        If PaletteHelper.IsLoaded Then Exit Sub

        Dim companyName = "Gadec"
        Dim appName = "AutoCAD App"
        Registerizer.Initialize(companyName, appName, "{0}\Settings".Compose(appName))
        Registerizer.MainSetting("AppDir", Registerizer.MainSetting("Bundle") & "\Contents")
        Dim customCodes = New Dictionary(Of String, String) From {
            {"Company", companyName},
            {"AppDir", Registerizer.MainSetting("AppDir")},
            {"Support", Registerizer.MainSetting("AppDir") & "\Support"},
            {"Resources", Registerizer.MainSetting("AppDir") & "\Resources"},
            {"AppDataFolder", "{AppData}\{0}\{1}".Compose(companyName, appName)},
            {"TempFolder", "{AppData}\{0}\{1}\Temp".Compose(companyName, appName)}
        }
        Composer.SetCustumCodes(customCodes)

        ApplicationServices.AddServices()

        My.Application.ChangeCulture("NL-NL")
        FileSystemHelper.CreateFolder("{AppDataFolder}".Compose)
        ApplicationEvents.Initialize()
        PaletteHelper.Load()
        Translator.Initialize("{Support}\SetLanguages.xml".Compose)
        PaletteHelper.Show()
        Dim menu = New GadecMenuHandler
        If menu.Available And (menu.NotLoaded Or menu.IsRenewed) Then menu.Load(ActiveEditor)
        If Registerizer.MainSetting("AppUpdate") = "Log-Showed" Then Exit Sub

        CopyPlotterFontAndTemplateFiles()
        SetAutoCADapplicationVariables()
        SetGadecFactorySettings()
        MessageBoxHistory()
    End Sub

    ''' <summary>
    ''' Shows the factory settings dialogbox.
    ''' </summary>
    ''' <param name="type">The type of setting. If none is specified, all settings are shown.</param>
    Public Sub SetGadecFactorySettings(Optional type As String = "")
        Dim settingsData = DataSetHelper.LoadFromXml("{Support}\SetDesignCenter.xml".Compose).GetTable("FactorySettings")
        If IsNothing(settingsData) Then Exit Sub

        Dim dialog = New SettingsDialog(settingsData, type)
    End Sub

    ''' <summary>
    ''' The main exception method for routing to the detailed exception dialogbox.
    ''' </summary>
    ''' <param name="exception">The exception to show.</param>
    Public Sub GadecException(exception As Exception)
        Select Case IsNothing(ActiveDocument)
            Case True
                exception.AddData($"No Active Document")
            Case Else
                exception.AddData($"Active document: {ActiveDocument.GetFileName}")
                exception.AddData($"Path: {ActiveDocument.GetPath}")
        End Select
        Dim assembly = Reflection.Assembly.GetExecutingAssembly
        Dim appName = "{0} {1}v{2}".Compose(assembly.GetName.Name, assembly.GetName.Version.Major, assembly.GetName.Version.Minor)
        Dim appBuild = Format(IO.File.GetLastWriteTime(assembly.Location), "dd-MM-yyyy - HH:mm:ss")
        MessageBoxException(exception, appName, appBuild, 1)
    End Sub

    'functions

    ''' <summary>
    ''' Gets the active document. 
    ''' </summary>
    ''' <returns>The active document.</returns>
    Public Function ActiveDocument() As Document
        Return Application.DocumentManager.MdiActiveDocument
    End Function

    ''' <summary>
    ''' Gets the active editor. 
    ''' </summary>
    ''' <returns>The active document.</returns>
    Public Function ActiveEditor() As Editor
        Return Application.DocumentManager.MdiActiveDocument.Editor
    End Function

    'private subs

    ''' <summary>
    ''' Copies the Gadec plotter-, font- and templatefiles to the AutoCAD folders.
    ''' </summary>
    Private Sub CopyPlotterFontAndTemplateFiles()
        Dim preferences As Autodesk.AutoCAD.Interop.AcadPreferences = Application.Preferences
        Dim folderPairs = New Dictionary(Of String, String) From {
            {"{AppDir}\Plotters".Compose, preferences.Files.PrinterConfigPath},
            {"{AppDir}\Plotters\Extended Frames Info".Compose, "{0}\Extended Frames Info".Compose(preferences.Files.PrinterConfigPath)},
            {"{AppDir}\Plotters\Plot Styles".Compose, preferences.Files.PrinterStyleSheetPath},
            {"{AppDir}\Plotters\PMP Files".Compose, preferences.Files.PrinterDescPath},
            {"{AppDir}\Fonts".Compose, ApplicationHelper.GetSupportPath()},
            {"{AppDir}\Template".Compose, preferences.Files.TemplateDwgPath}
        }
        For Each pair In folderPairs
            If Not IO.Directory.Exists(pair.Key) Then Continue For

            If Not IO.Directory.Exists(pair.Value) Then IO.Directory.CreateDirectory(pair.Value)
            For Each file In IO.Directory.GetFiles(pair.Key)
                Dim targetFile = "{0}\{1}".Compose(pair.Value, IO.Path.GetFileName(file))
                Select Case True
                    Case Not IO.File.Exists(targetFile) : IO.File.Copy(file, targetFile)
                    Case Not FileSystemHelper.FilesAreEqual(file, targetFile) : IO.File.Copy(file, targetFile, True)
                End Select
            Next
        Next
        preferences.Files.QNewTemplateFile = "{0}\Gadec_template.dwt".Compose(preferences.Files.TemplateDwgPath)
    End Sub

    ''' <summary>
    ''' Makes the Gadec bundle folder trusted and disables the proxy notice.
    ''' </summary>
    Private Sub SetAutoCADapplicationVariables()
        Dim doc = ActiveDocument()
        Dim trustedPathsString = SysVarHandler.GetVar("TRUSTEDPATHS").ToString
        Dim trustedPaths = trustedPathsString.Cut.ToList
        Select Case True
            Case trustedPathsString = "" : SysVarHandler.SetVar(doc, "TRUSTEDPATHS", "{0}\...".Compose(Registerizer.MainSetting("Bundle")))
            Case trustedPaths.Contains("{0}\...".Compose(Registerizer.MainSetting("Bundle")))
            Case Else
                trustedPaths.Add("{0}\...".Compose(Registerizer.MainSetting("Bundle")))
                SysVarHandler.SetVar(doc, "TRUSTEDPATHS", String.Join(";", trustedPaths))
        End Select
        SysVarHandler.SetVar(doc, "PROXYNOTICE", 0)
    End Sub

End Module
