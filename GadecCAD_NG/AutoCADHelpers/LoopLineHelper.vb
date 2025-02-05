'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.Geometry
Imports GadecCAD_NG.Extensions

''' <summary>
''' Provides methods to draw a lines between insertion points of blockreferences.
''' </summary>
Public Class LoopLineHelper

    'subs

    ''' <summary>
    ''' Creates a straight continuous polyline with 3 vertices, allowing the user to wrap it around other objects.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="startPoint">Startpoint of the loopline.</param>
    ''' <param name="endPoint">Endpoint of the loopline.</param>
    ''' <param name="layerId">The objectid of the layer to be used.</param>
    ''' <returns>The objectid of the new polyline.</returns>
    Public Shared Function CreateLoopLine(document As Document, startPoint As Point3d, endPoint As Point3d, layerId As ObjectId) As ObjectId
        Dim pieces = 3 'aantal delen, waaruit de polyline bestaat (mocht die aangepast moeten worden)
        Dim distanceX = endPoint.X - startPoint.X
        Dim distanceY = endPoint.Y - startPoint.Y

        Dim vertices = New Point2dCollection From {startPoint.GetPoint2d}
        For i = 1 To pieces - 1
            vertices.Add(New Point2d((startPoint.X + (distanceX * (i / pieces))), (startPoint.Y + +(distanceY * (i / pieces)))))
        Next
        vertices.Add(endPoint.GetPoint2d)

        Dim polyline = PolylineHelper.Create(vertices)
        Return EntityHelper.Add(document, polyline, layerId)
    End Function

    ''' <summary>
    ''' Creates a straight dashed line.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="startPoint">Startpoint of the loopline.</param>
    ''' <param name="endPoint">Endpoint of the loopline.</param>
    ''' <param name="layerId">The objectid of the layer to be used.</param>
    ''' <returns>The objectid of the new line.</returns>
    Public Shared Function CreateRemoteLine(document As Document, startPoint As Point3d, endPoint As Point3d, layerId As ObjectId) As ObjectId
        Dim lineTypeId = LineTypeHelper.GetLineTypeId(document.Database, "Hidden2")

        Return EntityHelper.Add(document, New Line(startPoint, endPoint), layerId, lineTypeId)
    End Function

End Class
