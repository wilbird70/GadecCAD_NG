'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.EditorInput
Imports Autodesk.AutoCAD.Geometry
Imports GadecCAD_NG.Extensions

''' <summary>
''' Provides methods for creating gaplines (drawing or converting). 
''' </summary>
Public Class GapLineMethods
    ''' <summary>
    ''' Default gap radius.
    ''' </summary>
    Private Shared ReadOnly _snap As Double = 1.25

    'subs

    ''' <summary>
    ''' Allows the user to draw a line that leaves or creates gaps in the horizontal intersecting lines that are on the same layer.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    Public Shared Sub Draw(document As Document)
        Dim db = document.Database
        Dim ed = document.Editor
        Dim promptPointOptions = New PromptPointOptions("Sel First Point:".Translate) With {.AllowNone = True}
        Using sysVar = New SysVarHandler(document)
            sysVar.Set("OSMODE", 0)
            sysVar.Set("SNAPMODE", 1)
            sysVar.Set("SNAPUNIT", New Point2d(1.25, 1.25))
            Dim selectPointResult = ed.GetPoint(promptPointOptions)
            sysVar.Set("ORTHOMODE", 1)
            If Not selectPointResult.Status = PromptStatus.OK Then Exit Sub

            Dim basePoint = selectPointResult.Value
            Do
                promptPointOptions.Message = "Sel Next Point:".Translate
                promptPointOptions.UseBasePoint = True
                promptPointOptions.BasePoint = basePoint
                selectPointResult = ed.GetPoint(promptPointOptions)
                If Not selectPointResult.Status = PromptStatus.OK Then Exit Do

                Dim line = New Line(basePoint, selectPointResult.Value)
                EntityHelper.Add(document, line, db.Clayer)
                Select Case line.Alignment
                    Case LineAlignment.horizontal
                        GapProcess(document, line.ObjectId)
                    Case LineAlignment.vertical
                        Dim selectionResult = ed.SelectCrossingWindow(line.StartPoint, line.EndPoint)
                        For Each entityId In selectionResult.Value.GetObjectIds
                            GapProcess(document, entityId, True)
                        Next
                End Select
                basePoint = selectPointResult.Value
            Loop
        End Using
    End Sub

    ''' <summary>
    ''' Allows the user to select horizontal lines interrupted by the vertical intersecting lines that are on the same layer.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    Public Shared Sub Cut(document As Document)
        Dim db = document.Database
        Dim ed = document.Editor
        Dim promptSelectionOptions = New PromptSelectionOptions With {.MessageForAdding = "Sel Lines:".Translate, .MessageForRemoval = "Rem Lines:".Translate}
        Dim selectionResult = ed.GetSelection(promptSelectionOptions)
        If Not selectionResult.Status = PromptStatus.OK Then Exit Sub

        For Each entityId In selectionResult.Value.GetObjectIds
            GapProcess(document, entityId)
        Next
    End Sub

    'private subs

    ''' <summary>
    ''' Processes the gapping of lines.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="lineId">Objectid of a line.</param>
    ''' <param name="onlyCurrentLayer">Whether to use only lines on the current layer.</param>
    Private Shared Sub GapProcess(document As Document, lineId As ObjectId, Optional onlyCurrentLayer As Boolean = False)
        Dim db = document.Database
        Dim ed = document.Editor
        Dim newLineIds = New DBObjectCollection
        Dim layerId As ObjectId
        Using document.LockDocument()
            Using tr = db.TransactionManager.StartTransaction
                Dim line = tr.GetLine(lineId, OpenMode.ForWrite)
                If IsNothing(line) Then Exit Sub

                layerId = line.LayerId
                If Not line.Alignment = LineAlignment.horizontal Then Exit Sub

                line.Sequence
                If Not line.Alignment = LineAlignment.horizontal OrElse line.Length < _snap Then Exit Sub

                Dim selectionResult = ed.SelectCrossingWindow(line.StartPoint, line.EndPoint)
                If Not selectionResult.Status = PromptStatus.OK Then Exit Sub

                Dim crossingLines = New Dictionary(Of Double, Line)
                For Each entityId In selectionResult.Value.GetObjectIds
                    Dim crossingLine = tr.GetLine(entityId, OpenMode.ForWrite)
                    If IsNothing(crossingLine) Then Continue For

                    crossingLine.Sequence
                    Select Case True
                        Case crossingLine.Length < _snap
                        Case Not line.LayerId = crossingLine.LayerId
                        Case onlyCurrentLayer AndAlso Not line.LayerId = db.Clayer
                        Case Not crossingLine.Alignment = LineAlignment.vertical
                        Case Else : crossingLines.TryAdd(crossingLine.StartPoint.X, crossingLine)
                    End Select
                Next
                If crossingLines.Count = 0 Then Exit Sub

                Dim crossingLineKeys = crossingLines.Keys.ToSortedList
                Dim pointY = line.StartPoint.Y
                Dim pointZ = line.StartPoint.Z
                Dim startPoint As Point3d
                For i = 0 To crossingLineKeys.Count - 1
                    Dim crossingLine = crossingLines(crossingLineKeys(i))
                    Dim onX = crossingLineKeys(i).GetPostionBetween(line.StartPoint.X, line.EndPoint.X)
                    Dim onY = pointY.GetPostionBetween(crossingLine.StartPoint.Y, crossingLine.EndPoint.Y)
                    Dim pointUp = New Point3d(crossingLineKeys(i), pointY + _snap, pointZ)
                    Dim pointDown = New Point3d(crossingLineKeys(i), pointY - _snap, pointZ)
                    Select Case onX
                        Case Between.onStartPoint
                            startPoint = New Point3d(crossingLineKeys(i) + _snap, pointY, pointZ)
                            Select Case onY
                                Case Between.onStartPoint : newLineIds.Add(New Line(pointUp, startPoint)) : crossingLine.StartPoint = pointUp
                                Case Between.onEndPoint : newLineIds.Add(New Line(pointDown, startPoint)) : crossingLine.EndPoint = pointDown
                                Case Between.onFirstHalf : newLineIds.Add(New Line(pointDown, startPoint))
                                Case Else : newLineIds.Add(New Line(pointUp, startPoint))
                            End Select
                            If crossingLineKeys.Count = 1 Then newLineIds.Add(New Line(startPoint, line.EndPoint))
                        Case Between.onEndPoint
                            If crossingLineKeys.Count = 1 Then startPoint = line.StartPoint
                            Dim endPoint = New Point3d(crossingLineKeys(i) - _snap, pointY, pointZ)
                            Select Case onY
                                Case Between.onStartPoint : newLineIds.Add(New Line(endPoint, pointUp)) : crossingLine.StartPoint = pointUp
                                Case Between.onEndPoint : newLineIds.Add(New Line(endPoint, pointDown)) : crossingLine.EndPoint = pointDown
                                Case Between.onFirstHalf : newLineIds.Add(New Line(endPoint, pointDown))
                                Case Else : newLineIds.Add(New Line(endPoint, pointUp))
                            End Select
                            newLineIds.Add(New Line(startPoint, endPoint))
                        Case Else
                            If i = 0 Then startPoint = line.StartPoint
                            Dim endPoint = New Point3d(crossingLineKeys(i) - _snap, pointY, pointZ)
                            newLineIds.Add(New Line(startPoint, endPoint))
                            startPoint = New Point3d(crossingLineKeys(i) + _snap, pointY, pointZ)
                            Select Case onY
                                Case Between.onStartPoint : crossingLine.StartPoint = pointUp : newLineIds.Add(New Line(endPoint, pointUp)) : newLineIds.Add(New Line(pointUp, startPoint))
                                Case Between.onEndPoint : crossingLine.EndPoint = pointDown : newLineIds.Add(New Line(endPoint, pointDown)) : newLineIds.Add(New Line(pointDown, startPoint))
                            End Select
                            If i = crossingLineKeys.Count - 1 Then newLineIds.Add(New Line(startPoint, line.EndPoint))
                    End Select
                Next
                tr.Commit()
            End Using
        End Using
        EntityHelper.Add(document, newLineIds, layerId, db.Celtype)
        EntityHelper.Delete(document, lineId)
    End Sub

End Class
