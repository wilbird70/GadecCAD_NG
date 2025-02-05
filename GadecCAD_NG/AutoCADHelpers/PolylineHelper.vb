'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.Geometry

''' <summary>
''' Provides methods for polylines.
''' </summary>
Public Class PolylineHelper

    ''' <summary>
    ''' Creates a polyline from, through, and to the specified points.
    ''' </summary>
    ''' <param name="vertices">The points (2D).</param>
    ''' <param name="closed">Closes the polyline if true.</param>
    ''' <returns>The polyline.</returns>
    Public Shared Function Create(vertices As Point2dCollection, Optional closed As Boolean = False) As Polyline
        Dim output = New Polyline
        For i = 0 To vertices.Count - 1
            output.AddVertexAt(0, vertices(i), 0, 0, 0)
        Next
        output.Closed = closed
        Return output
    End Function

End Class
