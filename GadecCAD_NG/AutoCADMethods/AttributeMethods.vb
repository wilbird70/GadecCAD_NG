'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.EditorInput
Imports GadecCAD_NG.Extensions

''' <summary>
''' Provides methods for manipulating the visibility and behavior of attribute references.
''' </summary>
Public Class AttributeMethods

    ''' <summary>
    ''' Allows the user to select attribute references to hide.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    Public Shared Sub Hide(document As Document)
        Dim db = document.Database
        Dim ed = document.Editor
        Dim promptNestedEntityOptions = New PromptNestedEntityOptions("Sel Attr:".Translate)
        Do
            Dim selectionResult = ed.GetNestedEntity(promptNestedEntityOptions)
            If Not selectionResult.Status = PromptStatus.OK Then Exit Do

            Dim objectId = selectionResult.ObjectId
            Autodesk.AutoCAD.Internal.Utils.SetUndoMark(True)
            Using tr = db.TransactionManager.StartTransaction
                Dim entity = tr.GetEntity(selectionResult.ObjectId)
                If Not entity.GetDBObjectType = "AttributeReference" Then Continue Do

                tr.Commit()
            End Using
            Autodesk.AutoCAD.Internal.Utils.SetUndoMark(False)
            AttributeHelper.SetVisibility(db, objectId, False)
        Loop
    End Sub

    ''' <summary>
    ''' Allows the user to select block references to show all of its attribute references.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    Public Shared Sub ShowAll(document As Document)
        Dim db = document.Database
        Dim ed = document.Editor
        Dim promptEntityOptions = New PromptEntityOptions("Sel Block:".Translate)
        Do
            Dim referenceIds = SelectMethods.GetSelectionOfReferences(document)
            If referenceIds.Count = 0 Then Exit Do

            Dim referenceData = ReferenceHelper.GetReferenceData(db, referenceIds)
            For Each row In referenceData.Rows.ToArray
                Dim columns = row.GetAttributeColumns
                Dim attributeIds = New ObjectIdCollection
                For Each column In columns
                    If row.HasAttribute(column) Then attributeIds.Add(row.GetAttributeId(column))
                Next
                AttributeHelper.SetVisibility(db, attributeIds, True)
            Next
        Loop
    End Sub

    ''' <summary>
    ''' Allows the user to select an attribute reference to unlock its position.
    ''' <para>Note: By escaping the first selection method, the user can select block references unlocking the attribute references tagged 'KKS'.</para>
    ''' </summary>
    ''' <param name="document">The present document.</param>
    Public Shared Sub UnlockPosition(document As Document)
        Dim db = document.Database
        Dim ed = document.Editor
        Dim promptNestedEntityOptions = New PromptNestedEntityOptions("Sel Attr:".Translate)
        Using tr = db.TransactionManager.StartTransaction
            Dim selectionResult = ed.GetNestedEntity(promptNestedEntityOptions)
            Select Case selectionResult.Status = PromptStatus.OK
                Case True
                    Dim entity = tr.GetEntity(selectionResult.ObjectId, OpenMode.ForWrite)
                    Select Case entity.GetDBObjectType
                        Case "AttributeReference" : entity.CastAsAttributeReference.LockPositionInBlock = False : Beep()
                        Case "AttributeDefinition" : entity.CastAsAttributeDefinition.LockPositionInBlock = False : Beep()
                    End Select
                Case Else
                    Dim referenceIds = SelectMethods.GetSelectionOfReferences(document)
                    Dim symbolData = ReferenceHelper.GetReferenceData(db, referenceIds)
                    For Each symbolRow In symbolData.Select
                        If Not symbolRow.HasValue("KKS") Then Continue For

                        tr.GetAttributeReference(symbolRow.GetAttributeId("KKS"), OpenMode.ForWrite).LockPositionInBlock = False
                        Beep()
                    Next
            End Select
            tr.Commit()
        End Using
    End Sub

End Class
