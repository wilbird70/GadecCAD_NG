'Gadec Engineerings Software (c) 2022

''' <summary>
''' Provides methods for loading and toggling the Gadec palette-set.
''' </summary>
Public Class PaletteHelper
    ''' <summary>
    ''' The Gadec palette-set.
    ''' </summary>
    Private Shared _paletteSet As Autodesk.AutoCAD.Windows.PaletteSet = Nothing
    ''' <summary>
    ''' The frames palette.
    ''' </summary>
    Private Shared _paletteFrames As FramesPalette
    ''' <summary>
    ''' The files palette.
    ''' </summary>
    Private Shared _paletteFiles As FilesPalette
    ''' <summary>
    ''' The settings palette.
    ''' </summary>
    Private Shared _paletteSettings As SettingsPalette
    ''' <summary>
    ''' The name of the current palette.
    ''' </summary>
    Private Shared _currentPalette As String

    'subs

    ''' <summary>
    ''' Loads the Gadec palette-set and adds the three palettes.
    ''' </summary>
    Public Shared Sub Load()
        Dim uid = New Guid("{ECBFEC73-9FE4-4aa2-8E4B-3068E94A2BFA}")
        _paletteSet = New Autodesk.AutoCAD.Windows.PaletteSet("Gadec", uid)
        _paletteFrames = New FramesPalette
        _paletteFiles = New FilesPalette
        _paletteSettings = New SettingsPalette
        _paletteSet.Add("Frames", _paletteFrames)
        _paletteSet.Add("Files", _paletteFiles)
        _paletteSet.Add("Settings", _paletteSettings)
        AddHandler _paletteSet.PaletteActivated, AddressOf PaletteActivatedEventHandler
        AddHandler Translator.LanguageChangedEvent, AddressOf LanguageChangedEventHandler
    End Sub

    ''' <summary>
    ''' Activates the frame palette and make the palette-set visible.
    ''' </summary>
    Public Shared Sub Show()
        _paletteSet.Activate(0)
        _currentPalette = _paletteSet.Item(0).Name
        _paletteSet.Visible = True
    End Sub

    ''' <summary>
    ''' Toggles the visibility of the Gadec palette-set and loads it if it is not already loaded.
    ''' </summary>
    Public Shared Sub ToggleVisibility()
        If IsNothing(_paletteSet) Then StartGadec() : Exit Sub

        _paletteSet.Visible = Not _paletteSet.Visible
    End Sub

    ''' <summary>
    ''' Reloads the framelist on the frames palette.
    ''' </summary>
    Public Shared Sub ReloadFrameList()
        If NotNothing(_paletteFrames) Then _paletteFrames.ReloadGridView()
    End Sub

    ''' <summary>
    ''' Reloads the filelist on the files palette.
    ''' </summary>
    Public Shared Sub ReloadFileList()
        If NotNothing(_paletteFiles) Then _paletteFiles.ReloadGridView()
    End Sub

    ''' <summary>
    ''' Shows the current layer on the settings palette.
    ''' </summary>
    Public Shared Sub ShowSelectedLayer()
        If NotNothing(_paletteSettings) Then _paletteSettings.SelectedLayer()
    End Sub

    'functions

    ''' <summary>
    ''' Gets the drawingnumber prefixlength to divide the system into groups.
    ''' </summary>
    ''' <returns>The prefixlength.</returns>
    Public Shared Function GetPrefixLength() As Integer
        Return _paletteFrames.GroupPrefixLength
    End Function

    ''' <summary>
    ''' Gets the index of the selected system.
    ''' </summary>
    ''' <returns>The index of the selected system.</returns>
    Public Shared Function GetSelectedGroupIndex() As Integer
        Return _paletteFrames.SelectedGroupIndex
    End Function

    ''' <summary>
    ''' Gets the location of the palette-sets titlebar.
    ''' </summary>
    ''' <returns>An integer representing the titlebarlocation.</returns>
    Public Shared Function GetTitlebarLocation() As Integer
        Return _paletteSet.TitleBarLocation
    End Function

    ''' <summary>
    ''' Gets the location (top left point) of the palette-set.
    ''' </summary>
    ''' <returns>A <see cref="Drawing.Point"/>.</returns>
    Public Shared Function GetLocation() As Drawing.Point
        Return _paletteSet.Location
    End Function

    ''' <summary>
    ''' Gets the size of the palette-set.
    ''' </summary>
    ''' <returns>A <see cref="Drawing.Size"/>.</returns>
    Public Shared Function GetSize() As Drawing.Size
        Return _paletteSet.Size
    End Function

    ''' <summary>
    ''' Detemine if the palette-set is already loaded.
    ''' </summary>
    ''' <returns>True if loaded.</returns>
    Public Shared Function IsLoaded() As Boolean
        Return NotNothing(_paletteSet)
    End Function

    ''' <summary>
    ''' Determine if the frames palette has the focus.
    ''' </summary>
    ''' <returns>True if it has the focus.</returns>
    Public Shared Function FramePageHasFocus() As Boolean
        Return _paletteSet.Item(0).Name = _currentPalette
    End Function

    ''' <summary>
    ''' Determine if the files palette has the focus.
    ''' </summary>
    ''' <returns>True if it has the focus.</returns>
    Public Shared Function FilesPageHasFocus() As Boolean
        Return _paletteSet.Item(1).Name = _currentPalette
    End Function

    'private eventHandlers

    ''' <summary>
    ''' EventHandler for the event that occurs when user selects another palette.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Shared Sub PaletteActivatedEventHandler(sender As Object, e As Autodesk.AutoCAD.Windows.PaletteActivatedEventArgs)
        _currentPalette = e.Activated.Name
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user changes the language.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Shared Sub LanguageChangedEventHandler(sender As Object, e As LanguageChangedEventArgs)
        _paletteSet(0).Name = "Frames".Translate
        _paletteSet(1).Name = "Files".Translate
        _paletteSet(2).Name = "Settings".Translate
    End Sub

End Class
