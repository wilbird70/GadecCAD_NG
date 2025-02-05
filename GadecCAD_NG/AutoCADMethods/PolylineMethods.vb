'Gadec Engineerings Software (c) 2022
Imports System.Windows.Forms
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.EditorInput
Imports Autodesk.AutoCAD.Geometry
Imports GadecCAD_NG.Extensions

''' <summary>
''' Provides methods with polylines, like creating a revisioncloud, show its area size and joining lines and arcs together in a polyline.
''' </summary>
Public Class PolylineMethods

    ''' <summary>
    ''' Allows the user to create a revisioncloud with arcs based on the current drawingscale (DIMSCALE).
    ''' </summary>
    ''' <param name="document">The present drawing.</param>
    Public Shared Sub CreateRevisionCloud(document As Document)
        Dim db = document.Database
        Dim ed = document.Editor
        Dim promptPointOptions = New PromptPointOptions("Sel First Point:".Translate) With {.AllowNone = True}
        Using sysVar = New SysVarHandler(document)
            sysVar.Set("OSMODE", 0)
            sysVar.Set("SNAPMODE", 0)
            Dim selectPointResult = ed.GetPoint(promptPointOptions)
            sysVar.Set("ORTHOMODE", 0)
            If Not selectPointResult.Status = PromptStatus.OK Then Exit Sub

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

                Dim temporaryLine = New Line(basePoint, selectPointResult.Value)
                temporaryLineIds.Add(EntityHelper.Add(document, temporaryLine, db.Clayer))
                basePoint = selectPointResult.Value
            Loop
            EntityHelper.Delete(document, temporaryLineIds)
            Dim polyline = PolylineHelper.Create(vertices, True)
            Dim polylineId = EntityHelper.Add(document, polyline, db.Clayer)
            Dim selection = SelectionSet.FromObjectIds(New ObjectId() {polylineId})
            Dim scale = sysVar.Get("DIMSCALE")
            ed.Command("_.REVCLOUD", "A", scale * 10, scale * 10, "", selection, "")
        End Using
    End Sub

    ''' <summary>
    ''' Allows the user to select polylines to show the area-size (m2) in a messagebox and has the option to insert it as text.
    ''' <para>If selecting polylines is skipped, the user can draw a rectangle to show its area-size.</para>
    ''' </summary>
    ''' <param name="document">The present drawing.</param>
    Public Shared Sub ShowAreaSize(document As Document)
        Dim db = document.Database
        Dim ed = document.Editor
        Dim temporaryObject = False
        Dim areas = New Dictionary(Of Entity, Double)
        Dim selectionResult = ed.GetSelection
        Select Case selectionResult.Status
            Case PromptStatus.OK
                Using tr = db.TransactionManager.StartTransaction()
                    For Each entityId In selectionResult.Value.GetObjectIds
                        Dim entity = tr.GetEntity(entityId)
                        Select Case entity.GetDBObjectType
                            Case "Polyline" : areas.TryAdd(entity, entity.CastAsPolyline.Area)
                            Case "Circle" : areas.TryAdd(entity, entity.CastAsCircle.Area)
                            Case "Ellipse" : areas.TryAdd(entity, entity.CastAsEllipse.Area)
                        End Select
                    Next
                    tr.Commit()
                End Using
            Case Else
                Dim promptPointOptions = New PromptPointOptions("Sel First Corner:".Translate) With {.AllowNone = True}
                Dim selectPointResult = ed.GetPoint(promptPointOptions)
                If Not selectPointResult.Status = PromptStatus.OK Then Exit Sub

                Dim basePoint = selectPointResult.Value
                Dim promptCornerOptions = New PromptCornerOptions("Sel Second Corner:".Translate, basePoint) With {.AllowNone = True}
                selectPointResult = ed.GetCorner(promptCornerOptions)
                If Not selectPointResult.Status = PromptStatus.OK Then Exit Sub

                Dim secondPoint = selectPointResult.Value
                Dim vertices = New Point2dCollection From {
                    New Point2d(basePoint.X, basePoint.Y),
                    New Point2d(basePoint.X, secondPoint.Y),
                    New Point2d(secondPoint.X, secondPoint.Y),
                    New Point2d(secondPoint.X, basePoint.Y)
                }
                Dim polyline = PolylineHelper.Create(vertices, True)
                areas.Add(polyline, polyline.Area)
                EntityHelper.Add(document, polyline, db.Clayer)
                temporaryObject = True
        End Select
        If areas.Count = 0 Then Exit Sub

        For Each pair In areas
            pair.Key.Highlight()
            ed.UpdateScreen()
            Dim areaString = (pair.Value / 1000000).ToFormatedString
            Dim messageResult = MessageBoxQuestion("CalcArea".Translate(areaString, "m²"), MessageBoxButtons.OKCancel)
            If messageResult = vbOK Then
                Dim promptPointOptions = New PromptPointOptions("Sel Point:".Translate) With {.AllowNone = True}
                Dim selectPointResult = ed.GetPoint(promptPointOptions)
                If selectPointResult.Status = PromptStatus.OK Then
                    Dim basePoint = selectPointResult.Value
                    Dim dbText = New DBText With {
                            .TextString = "{0} m2".Compose(areaString),
                            .Height = SysVarHandler.GetVar("DIMSCALE") * 7,
                            .Justify = AttachmentPoint.MiddleMid,
                            .Rotation = 0,
                            .AlignmentPoint = basePoint
                        }
                    Dim layerId = LayerHelper.GetLayerIdFromType(db, "Dim")
                    EntityHelper.Add(document, dbText, layerId)
                End If
            End If
            pair.Key.Unhighlight()
            If temporaryObject Then EntityHelper.Delete(document, pair.Key.ObjectId)
        Next
    End Sub

    ''' <summary>
    ''' Allows the user to select drawing objects to join them into a polyline, if possible.
    ''' </summary>
    ''' <param name="document"></param>
    Public Shared Sub Join(document As Document)
        Dim db = document.Database
        Dim curveIds = SelectMethods.GetSelectionOfCurves(document)
        If curveIds.Count = 0 Then Exit Sub

        Using document.LockDocument
            Using tr = db.TransactionManager.StartTransaction()
                Dim bt = tr.GetBlockTable(db.BlockTableId)
                Dim btr = tr.GetBlockTableRecord(bt(BlockTableRecord.ModelSpace), OpenMode.ForWrite)
                Dim curves = New List(Of Curve)
                Dim curvesToDelete = New List(Of Curve)
                For Each curveId In curveIds
                    Dim curve = tr.GetCurve(curveId, OpenMode.ForWrite)
                    Select Case curve.GetDBObjectType
                        Case "Polyline"
                            Dim entitySet = New DBObjectCollection
                            curve.Explode(entitySet)
                            curves.AddRange(entitySet.ToList)
                        Case Else : curves.Add(curve)
                    End Select
                    curvesToDelete.Add(curve)
                Next
                Dim parts = New List(Of List(Of Curve))
                Do While curves.Count > 0
                    Dim part = New List(Of Curve) From {curves.First}
                    Dim partEnds = New Line(curves.First.StartPoint, curves.First.EndPoint)
                    curves.Remove(curves.First)
                    Dim previousCount = curves.Count + 1
                    While curves.Count > 0 AndAlso curves.Count < previousCount
                        previousCount = curves.Count
                        For Each curve In curves
                            Dim action = 0
                            Select Case True
                                Case partEnds.EndPoint = curve.StartPoint : partEnds.EndPoint = curve.EndPoint : action = 1
                                Case partEnds.EndPoint = curve.EndPoint : partEnds.EndPoint = curve.StartPoint : action = 1
                                Case partEnds.StartPoint = curve.StartPoint : partEnds.StartPoint = curve.EndPoint : action = 2
                                Case partEnds.StartPoint = curve.EndPoint : partEnds.StartPoint = curve.StartPoint : action = 2
                            End Select
                            Select Case action
                                Case 1 : curves.Remove(curve) : part.Add(curve) : Exit For
                                Case 2 : curves.Remove(curve) : part.Insert(0, curve) : Exit For
                            End Select
                        Next
                    End While
                    part.Add(partEnds)
                    parts.Add(part)
                Loop
                For Each part In parts
                    Dim polyline = New Polyline()
                    polyline.SetPropertiesFrom(part.First)
                    Dim plane = New Plane(New Point3d(0, 0, 0), polyline.Normal)
                    Dim nextPoint As Point3d
                    Select Case part.First.StartPoint = part.Last.StartPoint
                        Case True
                            nextPoint = part.First.StartPoint
                            polyline.AddVertexAt(polyline.NumberOfVertices, part.First.StartPoint.Convert2d(plane), GetbulgeFromCurve(part.First, False), 0, 0)
                            polyline.AddVertexAt(polyline.NumberOfVertices, part.First.EndPoint.Convert2d(plane), 0, 0, 0)
                        Case Else
                            nextPoint = part.First.EndPoint
                            polyline.AddVertexAt(polyline.NumberOfVertices, part.First.EndPoint.Convert2d(plane), GetbulgeFromCurve(part.First, False), 0, 0)
                            polyline.AddVertexAt(polyline.NumberOfVertices, part.First.StartPoint.Convert2d(plane), 0, 0, 0)
                    End Select
                    polyline.Closed = part.Last.StartPoint = part.Last.EndPoint
                    part.Remove(part.Last)
                    For Each curve In part
                        Dim bulge = GetbulgeFromCurve(curve, curve.EndPoint = nextPoint)
                        polyline.SetBulgeAt(polyline.NumberOfVertices - 1, bulge)
                        Select Case curve.StartPoint = nextPoint
                            Case True : nextPoint = curve.EndPoint
                            Case Else : nextPoint = curve.StartPoint
                        End Select
                        polyline.AddVertexAt(polyline.NumberOfVertices, nextPoint.Convert2d(plane), 0, 0, 0)
                    Next
                    polyline.TransformBy(Matrix3d.PlaneToWorld(plane))
                    btr.AppendEntity(polyline)
                    tr.AddNewlyCreatedDBObject(polyline, True)
                Next
                curvesToDelete.ForEach(Sub(curve) curve.Erase())
                tr.Commit()
            End Using
        End Using
    End Sub

    'private functions

    ''' <summary>
    ''' Gets the bulge from the curve.
    ''' </summary>
    ''' <param name="curve">The curve.</param>
    ''' <param name="clockwise">The direction of the curve.</param>
    ''' <returns>The bulge.</returns>
    Private Shared Function GetbulgeFromCurve(ByVal curve As Curve, ByVal clockwise As Boolean) As Double
        Dim arc = TryCast(curve, Arc)
        If IsNothing(arc) Then Return 0.0

        Dim start = If(arc.StartAngle > arc.EndAngle, arc.StartAngle - 8 * Math.Atan(1), arc.StartAngle)
        Dim bulge = Math.Tan((arc.EndAngle - start) / 4)
        Return If(clockwise, -bulge, bulge)
    End Function

End Class
