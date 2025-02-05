'Gadec Engineerings Software (c) 2022
Imports System.Windows.Forms
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports GadecCAD_NG.Extensions

''' <summary>
''' Provides methods for blockreferences.
''' </summary>
Public Class ReferenceMethods

    'subs

    ''' <summary>
    ''' A method that allows the user to select blockreferences and change the scale.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    Public Shared Sub ChangeScale(document As Document)
        Dim referenceIds = SelectMethods.GetSelectionOfReferences(document)
        If referenceIds.Count = 0 Then Exit Sub

        Dim scale = DesignMethods.SetDrawingScale(document)
        ReferenceHelper.ChangeScale(document, referenceIds, scale)
    End Sub

    ''' <summary>
    ''' A method that allows the user to select blockreferences and change the rotation.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    Public Shared Sub ChangeRotation(document As Document)
        Dim referenceIds = SelectMethods.GetSelectionOfReferences(document)
        If referenceIds.Count = 0 Then Exit Sub

        Dim rotations = "RotateOptions".Translate.Cut
        Dim dialog = New ListBoxDialog("SelectRotation".Translate, rotations)
        Dim rotationInGrades = rotations(dialog.GetSelectedIndex).LeftString(3).ToDouble
        ReferenceHelper.ChangeRotation(document, referenceIds, rotationInGrades)
    End Sub

    ''' <summary>
    ''' A method that allows the user to select blockreferences and replace them with a different blockdefinition.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    Public Shared Sub Replace(document As Document)
        Dim db = document.Database
        Dim referenceIds = SelectMethods.GetSelectionOfReferences(document)
        If referenceIds.Count = 0 Then Exit Sub

        Dim definitionNames = New List(Of String)
        Using tr = db.TransactionManager.StartTransaction
            For Each referenceId In referenceIds
                Dim reference = tr.GetBlockReference(referenceId)
                Dim definitionId = If(reference.IsDynamicBlock, reference.DynamicBlockTableRecord, reference.BlockTableRecord)
                Dim definition = tr.GetBlockTableRecord(definitionId)
                If Not definitionNames.Contains(definition.Name) Then definitionNames.Add(definition.Name)
            Next
            tr.Commit()
        End Using
        Dim scale = If(db.ModelSpaceIsCurrent, CDbl(SysVarHandler.GetVar("DIMSCALE")), 1.0)
        Dim sessionList = "{0};;".Compose(Registerizer.UserSetting("DCsessionReplace")).Cut
        Dim dialog = New DesignCenter(sessionList, scale)
        If NotNothing(dialog.GetSession) Then Registerizer.UserSetting("DCsessionReplace", Join(dialog.GetSession, ";"))
        If Not dialog.DialogResult = DialogResult.OK Or dialog.GetBlockName = "" Then Exit Sub

        Using document.LockDocument
            Using tr = db.TransactionManager.StartTransaction
                Using import = New DefinitionsImporter("{Resources}\{0}".Compose(dialog.GetSourceFile))
                    Dim newDefinitionNames = import.ImportNestedDefinitions(document, dialog.GetBlockName)
                    RedefineReferences(document, newDefinitionNames)
                    Dim replaceList = New Dictionary(Of String, String)
                    For Each name In definitionNames
                        If newDefinitionNames.Count = 1 Then replaceList.TryAdd(name, newDefinitionNames(0)) : Continue For

                        Dim dialog2 = New ListBoxDialog("ReplaceBlocks".Translate(name), newDefinitionNames)
                        If dialog2.DialogResult = DialogResult.OK Then replaceList.TryAdd(name, newDefinitionNames(dialog2.GetSelectedIndex))
                    Next
                    Dim bt = tr.GetBlockTable(db.BlockTableId)
                    For Each referenceId In referenceIds
                        Dim reference = tr.GetBlockReference(referenceId, OpenMode.ForWrite)
                        Dim definitionId = If(reference.IsDynamicBlock, reference.DynamicBlockTableRecord, reference.BlockTableRecord)
                        Dim definition = tr.GetBlockTableRecord(definitionId)
                        If Not replaceList.ContainsKey(definition.Name) OrElse Not bt.Has(replaceList(definition.Name)) Then Continue For

                        reference.BlockTableRecord = bt(replaceList(definition.Name))
                        ReplaceAttributes(tr, reference, db)
                    Next
                End Using
                tr.Commit()
            End Using
        End Using
    End Sub

    ''' <summary>
    ''' A method that allows the user to select a blockdefinition to redefine.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    Public Shared Sub RedefineDefinitions(document As Document)
        Dim db = document.Database
        Dim scale = If(db.ModelSpaceIsCurrent, SysVarHandler.GetVar("DIMSCALE"), 1.0)
        Dim previousSession = "{0};;".Compose(Registerizer.UserSetting("DCsessionRedefine")).Cut
        Dim dialog = New DesignCenter(previousSession, scale)
        If NotNothing(dialog.GetSession) Then Registerizer.UserSetting("DCsessionRedefine", Join(dialog.GetSession, ";"))
        If Not dialog.DialogResult = DialogResult.OK Or dialog.GetBlockName = "" Then Exit Sub

        Using import = New DefinitionsImporter("{Resources}\{0}".Compose(dialog.GetSourceFile))
            Dim definitionNames = import.ImportNestedDefinitions(document, dialog.GetBlockName)
            RedefineReferences(document, definitionNames)
        End Using
    End Sub

    ''' <summary>
    ''' Redefines the blockreferences with the specified blocknames.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="blockNames">List of blocknames.</param>
    Public Shared Sub RedefineReferences(document As Document, blockNames As String())
        Dim db = document.Database
        Dim referenceIds = SelectHelper.GetAllReferencesInModelspace(document)
        If referenceIds.Count = 0 Then Exit Sub

        Using document.LockDocument
            Using tr = db.TransactionManager.StartTransaction
                Dim bt = tr.GetBlockTable(db.BlockTableId)
                For Each referenceId In referenceIds
                    Try
                        Dim reference = tr.GetBlockReference(referenceId, OpenMode.ForWrite)
                        Dim visibility = ReferenceVisibilityHelper.GetProperty(reference)
                        Dim definitionId = If(reference.IsDynamicBlock, reference.DynamicBlockTableRecord, reference.BlockTableRecord)
                        Dim definition = tr.GetBlockTableRecord(definitionId)
                        If Not blockNames.Contains(definition.Name) Or Not bt.Has(definition.Name) Then Continue For

                        reference.BlockTableRecord = definitionId
                        ReferenceVisibilityHelper.SetProperty(reference, visibility)
                        RedefineAttributes(tr, reference, db)
                    Catch ex As System.Exception
                        'do nothing 
                    End Try
                Next
                tr.Commit()
            End Using
        End Using
    End Sub

    ''' <summary>
    ''' Replaces the attributes of the specified blockreference.
    ''' </summary>
    ''' <param name="transaction">The current transaction.</param>
    ''' <param name="reference">The blockreference.</param>
    ''' <param name="alternateDatabaseToUse">The alternate drawing.</param>
    Public Shared Sub ReplaceAttributes(transaction As Transaction, reference As BlockReference, alternateDatabaseToUse As Database)
        Dim definition = transaction.GetBlockTableRecord(reference.BlockTableRecord)
        If Not definition.HasAttributeDefinitions Then Exit Sub

        Dim attributes = reference.AttributeCollection
        Dim attributeInfo = New Dictionary(Of String, KeyValuePair(Of ObjectId, String))
        For i = 0 To attributes.Count - 1
            Dim attribute = transaction.GetAttributeReference(attributes(i))
            attributeInfo.TryAdd(attribute.Tag, New KeyValuePair(Of ObjectId, String)(attribute.ObjectId, attribute.TextString))
        Next
        For Each entityId In definition
            Dim attributeDefinition = transaction.GetAttributeDefinition(entityId)
            If IsNothing(attributeDefinition) Then Continue For

            Dim textToChange = New Dictionary(Of ObjectId, String)
            Dim standardAttributes = "_LS_ADR_PNR_WNGD_LOC_ZONE_GRP_IN_OUT_KKS_SOFTWARE_HARDWARE_TEKST_EXIP_" '"_WD_EX_TYPE_"
            Select Case attributeInfo.ContainsKey(attributeDefinition.Tag)
                Case True
                    If Not standardAttributes.Contains("_{0}_".Compose(attributeDefinition.Tag)) Then
                        textToChange.TryAdd(attributeInfo(attributeDefinition.Tag).Key, attributeDefinition.TextString)
                    End If
                Case Else
                    Dim attribute = New AttributeReference()
                    attribute.SetDatabaseDefaults()
                    attribute.Tag = attributeDefinition.Tag
                    If attribute.Tag = "TYPE" AndAlso attribute.HasFields Then attribute.RemoveField()
                    attribute.SetAttributeFromBlock(attributeDefinition, reference.BlockTransform)
                    attribute.Position = attributeDefinition.Position.TransformBy(reference.BlockTransform)
                    attribute.AdjustAlignment(alternateDatabaseToUse)
                    attributes.AppendAttribute(attribute)
                    transaction.AddNewlyCreatedDBObject(attribute, True)
            End Select
            TextHelper.ChangeTextStrings(alternateDatabaseToUse, textToChange)
        Next
    End Sub

    'private subs

    ''' <summary>
    ''' Redefines the attributes of the specified blockreference.
    ''' </summary>
    ''' <param name="transaction">The current transaction.</param>
    ''' <param name="reference">The blockreference.</param>
    ''' <param name="alternateDatabaseToUse">The alternate drawing.</param>
    Private Shared Sub RedefineAttributes(transaction As Transaction, reference As BlockReference, alternateDatabaseToUse As Database)
        Dim definition = transaction.GetBlockTableRecord(reference.BlockTableRecord)
        If Not definition.HasAttributeDefinitions Then Exit Sub

        Dim attributes = reference.AttributeCollection
        Dim tags = New List(Of String)
        For i = 0 To attributes.Count - 1
            Dim attribute = transaction.GetAttributeReference(attributes(i))
            If Not tags.Contains(attribute.Tag) Then tags.Add(attribute.Tag)
        Next
        For Each entityId In definition
            Dim attributeDefinition = transaction.GetAttributeDefinition(entityId)
            If IsNothing(attributeDefinition) OrElse tags.Contains(attributeDefinition.Tag) Then Continue For

            Dim attribute As New AttributeReference()
            attribute.SetDatabaseDefaults()
            'optional
            attribute.Tag = attributeDefinition.Tag
            If attribute.Tag = "TYPE" AndAlso attribute.HasFields Then attribute.RemoveField()
            attribute.TextString = attributeDefinition.TextString
            attribute.SetAttributeFromBlock(attributeDefinition, reference.BlockTransform)
            attribute.Position = attributeDefinition.Position.TransformBy(reference.BlockTransform)
            attribute.AdjustAlignment(alternateDatabaseToUse)
            attributes.AppendAttribute(attribute)
            transaction.AddNewlyCreatedDBObject(attribute, True)
        Next
    End Sub

End Class
