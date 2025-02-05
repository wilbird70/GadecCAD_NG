'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.ApplicationServices

''' <summary>
''' Provides methods on the AutoCAD application level.
''' </summary>
Public Class ApplicationHelper
    ''' <summary>
    ''' Contains the main supportpath of AutoCAD.
    ''' <para>Note: Used for search one time only.</para>
    ''' </summary>
    Private Shared _supportPath As String = ""

    ''' <summary>
    ''' Gets the size of the AutoCAD application window.
    ''' </summary>
    ''' <returns>The window size.</returns>
    Public Shared Function WindowSize() As System.Windows.Size
        Return Core.Application.MainWindow.DeviceIndependentSize
    End Function

    ''' <summary>
    ''' Gets the main supportpath of AutoCAD.
    ''' </summary>
    ''' <returns>The main supportpath.</returns>
    Public Shared Function GetSupportPath() As String
        If Not _supportPath = "" Then Return _supportPath
        For Each supportPath In Application.Preferences.Files.SupportPath.ToString.Cut
            Select Case True
                Case Not supportPath.ToLower.EndsWith("\support")
                Case Not supportPath.ToLower.Contains("\appdata\")
                Case Else : _supportPath = supportPath : Exit For
            End Select
        Next
        Return _supportPath
    End Function

End Class
