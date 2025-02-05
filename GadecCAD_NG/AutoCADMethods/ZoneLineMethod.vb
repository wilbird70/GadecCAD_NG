'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.EditorInput
Imports Autodesk.AutoCAD.Geometry
Imports GadecCAD_NG.Extensions

''' <summary>
''' Provides a method that allows the user to draw a blue zonal line (closed polyline).
''' </summary>
Public Class ZoneLineMethod

    ''' <summary>
    ''' Allows the user to draw a blue zonal line (closed polyline).
    ''' </summary>
    ''' <param name="document">The present document.</param>
    Public Shared Sub Draw(document As Document)
        Dim ed = document.Editor
        Dim db = document.Database
        Dim layerId = LayerHelper.GetLayerIdFromType(db, "Line")
        Dim promptPointOptions = New PromptPointOptions("Sel First Point:".Translate) With {.AllowNone = True}
        Using sysVar = New SysVarHandler(document)
            sysVar.Set("OSMODE", 0)
            sysVar.Set("SNAPMODE", 0)
            Dim selectPointResult = ed.GetPoint(promptPointOptions)
            sysVar.Set("ORTHOMODE", 1)
            If selectPointResult.Status = PromptStatus.OK Then
                Dim scale = sysVar.Get("DIMSCALE")
                Dim temporaryLineIds = New ObjectIdCollection
                Dim vertices = New Point2dCollection
                Dim basePoint = selectPointResult.Value
                Do
                    vertices.Add(basePoint.GetPoint2d)
                    promptPointOptions.Message = "Sel Next Point:".Translate
                    promptPointOptions.UseBasePoint = True
                    promptPointOptions.BasePoint = basePoint
                    selectPointResult = ed.GetPoint(promptPointOptions)
                    If Not selectPointResult.Status = PromptStatus.OK Then Exit Do

                    'temporary polylines
                    Dim nextPoint = selectPointResult.Value
                    Dim tempPolyLine = PolylineHelper.Create(New Point2dCollection({basePoint.GetPoint2d, nextPoint.GetPoint2d}))
                    tempPolyLine.ConstantWidth = scale * 2
                    tempPolyLine.Color = Autodesk.AutoCAD.Colors.Color.FromRgb(0, 0, 255)
                    temporaryLineIds.Add(EntityHelper.Add(document, tempPolyLine, layerId))

                    basePoint = nextPoint
                Loop
                EntityHelper.Delete(document, temporaryLineIds)
                Dim polyline = PolylineHelper.Create(vertices, True)
                polyline.ConstantWidth = scale * 2
                polyline.Color = Autodesk.AutoCAD.Colors.Color.FromRgb(0, 0, 255)
                Dim polylineId = EntityHelper.Add(document, polyline, layerId)
                DrawOrderHelper.SendToBack(document, New ObjectIdCollection From {polylineId})
            End If
        End Using
    End Sub

End Class
