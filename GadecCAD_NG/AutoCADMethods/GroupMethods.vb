'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.EditorInput
Imports Autodesk.AutoCAD.Geometry
Imports GadecCAD_NG.Extensions

''' <summary>
''' Provides a method to stretch groups.
''' </summary>
Public Class GroupMethods

    ''' <summary>
    ''' Allows the user to select a group of blockreferences that can be stretched by 5 units each.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    Public Shared Sub Stretch(document As Document)
        Dim db = document.Database
        Dim ed = document.Editor
        Dim promptEntityOptions = New PromptEntityOptions("Sel Block:".Translate)
        Do
            Dim selectionResult = ed.GetEntity(promptEntityOptions)
            If Not selectionResult.Status = PromptStatus.OK Then Exit Do

            Dim referenceIds = GroupHelper.GetGroupedEntityIds(db, selectionResult.ObjectId)
            Dim symbolData = ReferenceHelper.GetReferenceData(db, referenceIds)
            If symbolData.Rows.Count = 0 Then MessageBoxInfo("NoGroupWithBlocks".Translate) : Continue Do

            Using sysVar = New SysVarHandler(document)
                sysVar.Set("ORTHOMODE", 1)
                Dim row = symbolData.Rows(0)
                Dim promptAngleOptions = New PromptAngleOptions("StretchTo".Translate) With {.BasePoint = row.GetValue("Position"), .UseBasePoint = True, .AllowNone = True}
                Dim selectAngleResult = ed.GetAngle(promptAngleOptions)
                If selectAngleResult.Status = PromptStatus.OK Then
                    Dim angle = selectAngleResult.Value
                    Using tr = db.TransactionManager.StartTransaction
                        Dim reference = tr.GetBlockReference(row.GetValue("ObjectID"))
                        Dim scale = reference.ScaleFactors.X
                        For i = 0 To symbolData.Rows.Count - 1
                            row = symbolData.Rows(i)
                            reference = tr.GetBlockReference(row.GetValue("ObjectID"), OpenMode.ForWrite)
                            Dim basePoint = New Point3d(0, 0, 0).GetPolarPoint(angle, i * 5 * scale)
                            reference.TransformBy(Matrix3d.Displacement(basePoint.GetAsVector))
                        Next
                        If reference.IsDynamicBlock Then
                            Dim propertyCollection = reference.DynamicBlockReferencePropertyCollection
                            Dim basePoint = New Point3d(0, 0, 0).GetPolarPoint(angle - reference.Rotation, (referenceIds.Count - 1) * 5 * scale)
                            For Each referenceProperty In propertyCollection.ToArray
                                Select Case referenceProperty.PropertyName
                                    Case "Position1 X" : referenceProperty.Value -= basePoint.X
                                    Case "Position1 Y" : referenceProperty.Value -= basePoint.Y
                                End Select
                            Next
                        End If
                        tr.Commit()
                    End Using
                End If
            End Using
        Loop
    End Sub

End Class
