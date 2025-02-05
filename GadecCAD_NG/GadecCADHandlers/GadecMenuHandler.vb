'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.EditorInput

''' <summary>
''' <para><see cref="GadecMenuHandler"/> checks if the Gadec-menu is available and if it has been loaded or modified.</para>
''' <para>Also provides a method to (re)load the Gadec-menu.</para>
''' </summary>
Public Class GadecMenuHandler
    ''' <summary>
    ''' Determine if Gadec-menu is available.
    ''' </summary>
    ''' <returns>True if available.</returns>
    Public ReadOnly Property Available As Boolean = False
    ''' <summary>
    ''' Determine if Gadec-menu is loaded.
    ''' </summary>
    ''' <returns>True if not loaded.</returns>
    Public ReadOnly Property NotLoaded As Boolean = False
    ''' <summary>
    ''' Determine if Gadec-menu is renewed by update.
    ''' </summary>
    ''' <returns>True if renewed.</returns>
    Public ReadOnly Property IsRenewed As Boolean = False

    ''' <summary>
    ''' The main AutoCAD customization section.
    ''' </summary>
    Private ReadOnly _customizationSection As Autodesk.AutoCAD.Customization.CustomizationSection
    ''' <summary>
    ''' Holds the AutoCAD support-path.
    ''' </summary>
    Private ReadOnly _supportPath As String
    ''' <summary>
    ''' The fullname of the Gadec-menufile in the bundle (source) folder.
    ''' </summary>
    Private ReadOnly _sourceCuix As String
    ''' <summary>
    ''' The fullname of the copy of Gadec-menufile in the AutoCAD support folder.
    ''' </summary>
    Private ReadOnly _targetCuix As String

    ''' <summary>
    ''' Initializes a new instance of <see cref="GadecMenuHandler"/>.
    ''' <para><see cref="GadecMenuHandler"/> checks if the Gadec-menu is available and if it has been loaded or modified.</para>
    ''' <para>Also provides a method to (re)load the Gadec-menu.</para>
    ''' </summary>
    Public Sub New()
        _customizationSection = New Autodesk.AutoCAD.Customization.CustomizationSection("{0}.cuix".Compose(SysVarHandler.GetVar("MENUNAME")))
        _supportPath = ApplicationHelper.GetSupportPath
        _sourceCuix = "{Support}\Gadec_mnu.cuix".Compose
        _targetCuix = "{0}\Gadec_mnu.cuix".Compose(_supportPath)
        If IO.File.Exists(_sourceCuix) Then
            _Available = True
            If Not _customizationSection.PartialCuiFiles.Contains(_targetCuix) Then _NotLoaded = True
            If Not FileSystemHelper.FilesAreEqual(_sourceCuix, _targetCuix) Then _IsRenewed = True
        End If
    End Sub

    ''' <summary>
    ''' Loads or reloads the Gadec-menu.
    ''' </summary>
    ''' <param name="editor">The present editor.</param>
    Public Sub Load(editor As Editor)
        Dim dialog = New ListBoxDialog("Toolbars...", "Zichtbaar maken;Verbergen;Locatie herstellen".NotYetTranslated.Cut)
        Dim toolbarOption = If(dialog.DialogResult = System.Windows.Forms.DialogResult.OK, dialog.GetSelectedIndex, -1)

        FileSystemHelper.CreateFolder(_supportPath)
        FileSystemHelper.DeleteFile(_targetCuix)
        FileSystemHelper.DeleteFile("{0}\Gadec_mnu.mnr".Compose(_supportPath))
        FileSystemHelper.DeleteFile("{0}\Gadec_mnu_light.mnr".Compose(_supportPath))

        IO.File.Copy(_sourceCuix, _targetCuix)
        editor.Command("CUIUNLOAD", "GADEC")
        editor.Command("CUILOAD", _targetCuix)

        Dim currentWorkspaceName = Application.GetSystemVariable("WSCURRENT").ToString
        Dim cuixGadec = New Autodesk.AutoCAD.Customization.CustomizationSection(_targetCuix)
        Dim rootGadec = _customizationSection.MenuGroup.RibbonRoot
        Dim tabGadec = rootGadec.FindTab("ID_GADECRIBBON")
        Dim currentWorkspace = _customizationSection.getWorkspace(currentWorkspaceName)
        _customizationSection.MergeOrAddTabToWorkspace(currentWorkspace, tabGadec)
        If _customizationSection.IsModified Then _customizationSection.Save() : Application.ReloadAllMenus()
        If toolbarOption < 0 Then Exit Sub

        Dim menuGroup As Autodesk.AutoCAD.Interop.AcadMenuGroup = Application.MenuGroups.Item("GADEC")
        For i = 0 To menuGroup.Toolbars.Count - 1
            Dim toolbar = menuGroup.Toolbars.Item(i)
            Select Case toolbarOption
                Case 0 : toolbar.Visible = True
                Case 1 : toolbar.Visible = False
                Case 2 : toolbar.Visible = True : toolbar.Float(200 + (i * 50), 200 + (i * 50), 1)
            End Select
        Next
    End Sub

End Class
