'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.Runtime

Public Module MenuCommands

    ''' <summary>
    ''' The command where Gadec AutoCAD Application begins.
    ''' <para>Note: This command is mentioned in the 'PackageContents.xml' file in the rootdirectory of the Bundle.</para>
    ''' </summary>
    <CommandMethod("GadecPalette")>
    Public Sub commandGadecPalette()
        Try
            Main.StartGadec()
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' The command to change the palette's visibility.
    ''' <para>Note: Shortcut Ctrl-5 uses this command to toggle the visibility.</para>
    ''' </summary>
    <CommandMethod("GADECSHOW")>
    Public Sub commandGadecShow()
        Try
            PaletteHelper.ToggleVisibility()
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' Command to reload the Gadec Menu.
    ''' <para>See: Reload menu button on the palette (tab Settings).</para>
    ''' </summary>
    <CommandMethod("RELOADGADEC")>
    Public Sub commandReloadGadecMenu()
        Try
            'registerizer.MainSetting("MenuUpdate", "Reload")
            Dim menu = New GadecMenuHandler
            If menu.Available Then menu.Load(ActiveEditor)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

End Module
