'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.Geometry

''' <summary>
''' Provides methods for setting the drawing snap.
''' </summary>
Public Class SnapHelper

    ''' <summary>
    ''' Sets to snap mode and set snap setting to 1.25.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    Public Shared Sub Set125(document As Document)
        SysVarHandler.SetVar(document, "SNAPMODE", 1)
        SysVarHandler.SetVar(document, "SNAPUNIT", New Point2d(1.25, 1.25))
    End Sub

    ''' <summary>
    ''' Sets to snap mode and halves current snap setting.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    Public Shared Sub HalveIt(document As Document)
        Dim point As Point2d = SysVarHandler.GetVar("SNAPUNIT")
        SysVarHandler.SetVar(document, "SNAPMODE", 1)
        SysVarHandler.SetVar(document, "SNAPUNIT", New Point2d(point.X / 2, point.Y / 2))
    End Sub

    ''' <summary>
    ''' Sets to snap mode and doubles current snap setting.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    Public Shared Sub DoubleIt(document As Document)
        Dim point As Point2d = SysVarHandler.GetVar("SNAPUNIT")
        SysVarHandler.SetVar(document, "SNAPMODE", 1)
        SysVarHandler.SetVar(document, "SNAPUNIT", New Point2d(point.X * 2, point.Y * 2))
    End Sub

End Class
