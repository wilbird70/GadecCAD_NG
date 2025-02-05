'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.Geometry
Imports Autodesk.AutoCAD.GraphicsInterface

''' <summary>
''' <para><see cref="TransientHandler"/> can show temporary transient objects to the dwg viewer.</para>
''' </summary>
Public Class TransientHandler
    Implements IDisposable
    Private _disposed As Boolean

    ''' <summary>
    ''' Contains the collection of created transients.
    ''' </summary>
    Private _transients As Dictionary(Of DBObject, IntegerCollection)

    ''' <summary>
    ''' Initializes a new instance of <see cref="TransientHandler"/>.
    ''' <para><see cref="TransientHandler"/> can show temporary transient objects to the dwg viewer.</para>
    ''' </summary>
    Public Sub New()
        _transients = New Dictionary(Of DBObject, IntegerCollection)
    End Sub

    ''' <summary>
    ''' Hides all the transient objects.
    ''' </summary>
    ''' <param name="disposing"></param>
    Protected Overridable Sub Dispose(disposing As Boolean)
        If _disposed Then Exit Sub

        If disposing Then
            For Each pair In _transients
                TransientManager.CurrentTransientManager.EraseTransient(pair.Key, pair.Value)
                pair.Key.Dispose()
            Next
            _transients = Nothing
        End If
        _disposed = True
    End Sub

    ''' <summary>
    ''' Implements the dispose method.
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
        Dispose(disposing:=True)
        GC.SuppressFinalize(Me)
    End Sub

    'functions

    ''' <summary>
    ''' Displays a temporary magenta circle.
    ''' </summary>
    ''' <param name="midPoint">The middele point of the circle.</param>
    ''' <param name="radius">The radius of the circle.</param>
    ''' <param name="highlight">If true, shows highlighted.</param>
    ''' <returns>True if not failed.</returns>
    Public Function ShowCircle(midPoint As Point3d, radius As Double, highlight As Boolean) As Boolean
        Dim circle = New Circle()
        Dim viewportNumbers = New IntegerCollection
        circle.Center = midPoint
        circle.Radius = radius
        circle.ColorIndex = 6
        Try
            Dim drawingMode = If(highlight, TransientDrawingMode.Highlight, TransientDrawingMode.DirectTopmost)
            TransientManager.CurrentTransientManager.AddTransient(circle, drawingMode, 128, viewportNumbers)
            _transients.Add(circle, viewportNumbers)
            Return True
        Catch ex As Exception
            ex.AddData($"({midPoint})")
            ex.AddData($"r{radius}")
            ex.Rethrow
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Displays a temporary magenta arc (spline).
    ''' <para>Note: The midpoint is calculated by adding a quarter of the x-distance to the y-value of the actual midpoint.</para>
    ''' </summary>
    ''' <param name="startPoint">The startpoint of the arc.</param>
    ''' <param name="endPoint">The endpoint of the arc.</param>
    ''' <param name="highlight">If true, shows highlighted.</param>
    ''' <returns>True if not failed.</returns>
    Public Function ShowArc(startPoint As Point3d, endPoint As Point3d, highlight As Boolean) As Boolean
        Dim distanceX = endPoint.X - startPoint.X
        Dim distanceY = endPoint.Y - startPoint.Y
        Dim midPoint = New Point3d(startPoint.X + (distanceX / 2), startPoint.Y + (distanceY / 2) + (distanceX / 4), 0)
        Dim spline = New Spline(New Point3dCollection({startPoint, midPoint, endPoint}), 4, 0)
        Dim viewportNumbers = New IntegerCollection
        spline.ColorIndex = 6
        Try
            Dim drawingMode = If(highlight, TransientDrawingMode.Highlight, TransientDrawingMode.DirectTopmost)
            TransientManager.CurrentTransientManager.AddTransient(spline, drawingMode, 128, viewportNumbers)
            _transients.Add(spline, viewportNumbers)
            Return True
        Catch ex As Exception
            ex.AddData($"({startPoint})")
            ex.AddData($"({endPoint})")
            ex.Rethrow
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Displays a temporary magenta line.
    ''' </summary>
    ''' <param name="startPoint">The startpoint of the line.</param>
    ''' <param name="endPoint">The endpoint of the line.</param>
    ''' <param name="highlight">If true, shows highlighted.</param>
    ''' <returns>True if not failed.</returns>
    Public Function ShowLine(startPoint As Point3d, endPoint As Point3d, highlight As Boolean) As Boolean
        Dim line = New Line(startPoint, endPoint)
        Dim viewportNumbers = New IntegerCollection
        line.ColorIndex = 6
        Try
            Dim drawingMode = If(highlight, TransientDrawingMode.Highlight, TransientDrawingMode.DirectTopmost)
            TransientManager.CurrentTransientManager.AddTransient(line, drawingMode, 128, viewportNumbers)
            _transients.Add(line, viewportNumbers)
            Return True
        Catch ex As Exception
            ex.AddData($"({startPoint})")
            ex.AddData($"({endPoint})")
            ex.Rethrow
            Return False
        End Try
    End Function

End Class
