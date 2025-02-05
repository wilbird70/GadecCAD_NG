'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports GadecCAD_NG.Extensions

Public Class AddOn
    Implements Autodesk.AutoCAD.Runtime.IExtensionApplication

    'subs

    Public Sub Initialize() Implements Autodesk.AutoCAD.Runtime.IExtensionApplication.Initialize
        ContextMenu.Attach()
    End Sub

    Public Sub Terminate() Implements Autodesk.AutoCAD.Runtime.IExtensionApplication.Terminate
        ContextMenu.Detach()
    End Sub

    'private subs

    Private Shared Sub ps_Load(ByVal sender As Object, ByVal e As Autodesk.AutoCAD.Windows.PalettePersistEventArgs)
        Dim a = CType(e.ConfigurationSection.ReadProperty("Gadec", 22.3), Double)
    End Sub

    Private Shared Sub ps_Save(ByVal sender As Object, ByVal e As Autodesk.AutoCAD.Windows.PalettePersistEventArgs)
        e.ConfigurationSection.WriteProperty("Gadec", 32.3)
    End Sub

End Class

Public Class ContextMenu
    Private Shared _ContextMenuExtension As Autodesk.AutoCAD.Windows.ContextMenuExtension

    'subs

    Public Shared Sub Attach()
        _ContextMenuExtension = New Autodesk.AutoCAD.Windows.ContextMenuExtension()
        Dim menuItem As New Autodesk.AutoCAD.Windows.MenuItem("GroupStretch")
        AddHandler menuItem.Click, AddressOf GroupStretchClickEventHandler
        _ContextMenuExtension.MenuItems.Add(menuItem)
        Dim rxClass As Autodesk.AutoCAD.Runtime.RXClass = Entity.GetClass(GetType(Entity))
        Application.AddObjectContextMenuExtension(rxClass, _ContextMenuExtension)
    End Sub

    Public Shared Sub Detach()
        Dim rxClass As Autodesk.AutoCAD.Runtime.RXClass = Entity.GetClass(GetType(Entity))
        Application.RemoveObjectContextMenuExtension(rxClass, _ContextMenuExtension)
    End Sub

    'private subs

    Private Shared Sub GroupStretchClickEventHandler(sender As Object, e As EventArgs)
        Dim doc = ActiveDocument()
        doc.SendString("_.GROUPSTRETCH ")
    End Sub

End Class
