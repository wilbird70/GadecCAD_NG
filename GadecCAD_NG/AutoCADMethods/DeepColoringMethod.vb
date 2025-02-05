'Gadec Engineerings Software (c) 2022
Imports System.Windows.Forms
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.EditorInput
Imports GadecCAD_NG.Extensions

''' <summary>
''' Provides a method that allows the user to select entities to pick a new color and colors also its nested entities.
''' </summary>
Public Class DeepColoringMethod

    ''' <summary>
    ''' Runs the method that allows the user to select entities to pick a new color and colors also its nested entities.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    Public Shared Sub Start(document As Document)
        Dim db = document.Database
        Dim ed = document.Editor
        Dim promptSelectionOptions = New PromptSelectionOptions
        promptSelectionOptions.MessageForAdding = "Sel Entities:".Translate
        promptSelectionOptions.MessageForRemoval = "Rem Entities:".Translate
        Dim selectionResult = ed.GetSelection(promptSelectionOptions)
        If Not selectionResult.Status = PromptStatus.OK Then Exit Sub

        Dim entityIds = selectionResult.Value.GetObjectIds.ToList
        Dim colorDialog = New Autodesk.AutoCAD.Windows.ColorDialog
        If Not colorDialog.ShowDialog = DialogResult.OK Then Exit Sub

        Dim chosenColor = colorDialog.Color
        Dim errorCount = 0
        Using document.LockDocument
            Using tr = db.TransactionManager.StartTransaction
                Dim definitionIds = New ObjectIdCollection
                Dim count = 0
                Do Until count > entityIds.Count - 1
                    Try
                        'kleur geven aan de entiteit
                        Dim entityId = entityIds(count)
                        Dim entity = tr.GetEntity(entityId, OpenMode.ForWrite, True)
                        entity.Color = chosenColor
                        'als entiteit is opgebouwd uit afzonderlijke entiteiten
                        Select Case entity.GetDBObjectType
                            Case "BlockReference"
                                Dim reference = entity.CastAsBlockReference
                                'attribuutreferenties
                                For Each attributeId In reference.AttributeCollection.ToArray
                                    tr.GetAttributeReference(attributeId, OpenMode.ForWrite).Color = chosenColor
                                Next
                                'entiteiten toevoegen aan de te kleuren collectie
                                Dim definitionId = reference.BlockTableRecord
                                If Not definitionIds.Contains(definitionId) Then
                                    definitionIds.Add(definitionId)
                                    Dim btr = tr.GetBlockTableRecord(definitionId)
                                    entityIds.AddRange(btr.ToArray)
                                End If
                            Case "AlignedDimension", "ArcDimension", "DiametricDimension",
                                 "LineAngularDimension2", "Point3AngularDimension", "RadialDimension",
                                 "RadialDimensionLarge", "RotatedDimension"
                                'onderdelen van dimensies
                                Dim dimension = entity.CastAsDimension
                                dimension.Dimclrd = chosenColor
                                dimension.Dimclre = chosenColor
                                dimension.Dimclrt = chosenColor
                        End Select
                    Catch
                        errorCount += 1
                    End Try
                    count += 1
                Loop
                tr.Commit()
            End Using
        End Using
        If errorCount > 0 Then MsgBox(("EntitiesFailed").Translate(errorCount))
        ed.Regen()
    End Sub

End Class
