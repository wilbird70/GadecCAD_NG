'Gadec Engineerings Software (c) 2022
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms

''' <summary>
''' <para><see cref="OutlinedLabel"/> represents a standaard Windows label with outlined font.</para>
''' </summary>
Public Class OutlinedLabel
    Inherits Label

    ''' <summary>
    ''' Contains the color of the outline border.
    ''' </summary>
    Private ReadOnly _outlineColor As Color
    ''' <summary>
    ''' Contains the thickness of the outline border.
    ''' </summary>
    Private ReadOnly _outlineThickness As Integer

    ''' <summary>
    ''' Initializes a new instance of <see cref="OutlinedLabel"/>.
    ''' <para><see cref="OutlinedLabel"/> represents a standaard Windows label with outlined font.</para>
    ''' </summary>
    ''' <param name="outlineColor">The color of the outline border.</param>
    ''' <param name="outlineThickness">The thickness of the outline border.</param>
    Public Sub New(outlineColor As Color, outlineThickness As Integer)
        MyBase.New
        _outlineColor = outlineColor
        _outlineThickness = outlineThickness
    End Sub

    ''' <summary>
    ''' The new (overridden) onpaint eventhandler.
    ''' </summary>
    ''' <param name="e"></param>
    Protected Overrides Sub OnPaint(ByVal e As PaintEventArgs)
        e.Graphics.FillRectangle(New SolidBrush(BackColor), ClientRectangle)
        Dim graphicsPath = New GraphicsPath
        Dim pen = New Pen(_outlineColor, _outlineThickness)
        Dim stringFormat = New StringFormat
        Dim solidBrush = New SolidBrush(ForeColor)
        graphicsPath.AddString(Text, Font.FontFamily, CType(Font.Style, Integer), Font.Size, ClientRectangle, stringFormat)
        e.Graphics.ScaleTransform(1.3!, 1.35!)
        e.Graphics.SmoothingMode = SmoothingMode.HighQuality
        e.Graphics.DrawPath(pen, graphicsPath)
        e.Graphics.FillPath(solidBrush, graphicsPath)
    End Sub

End Class
