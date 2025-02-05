'Gadec Engineerings Software (c) 2022
Imports System.Windows.Forms
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.EditorInput
Imports GadecCAD_NG.Extensions

''' <summary>
''' Provides methods for the user to manipulate text objects.
''' </summary>
Public Class TextMethods

    ''' <summary>
    ''' Allows the user to edit any text, if it is editable.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    Public Shared Sub EditEveryText(document As Document)
        Dim db = document.Database
        Dim ed = document.Editor
        Dim promptNestedEntityOptions = New PromptNestedEntityOptions("Sel Some Text:".Translate)
        Dim entity As Entity
        Dim entityIsInModelOrPaperspace As Boolean
        Do
            Dim hasAttributes = False
            Dim nestedEntityResult = ed.GetNestedEntity(promptNestedEntityOptions)
            If Not nestedEntityResult.Status = PromptStatus.OK Then Exit Do

            Dim entityId = nestedEntityResult.ObjectId
            Using tr = db.TransactionManager.StartTransaction
                entity = tr.GetEntity(entityId)
                Dim btr = tr.GetBlockTableRecord(entity.OwnerId)
                If NotNothing(btr) Then hasAttributes = btr.HasAttributeDefinitions
                entityIsInModelOrPaperspace = tr.EntityIsInModelOrPaperspace(entity)
                tr.Commit()
            End Using
            If entityIsInModelOrPaperspace Then
                Select Case entity.GetDBObjectType
                    Case "DBText"
                        Dim dialog = New InputBoxDialog("DBtext".Translate, entity.CastAsDBText.TextString)
                        If Not dialog.DialogResult = DialogResult.OK Then Continue Do

                        Dim textToChange = New Dictionary(Of ObjectId, String) From {{entityId, dialog.InputText}}
                        TextHelper.ChangeTextStrings(document, textToChange)
                        Continue Do
                    Case "MText", "AttributeDefinition"
                        Using sysVar = New SysVarHandler(document)
                            sysVar.Set("TEXTED", 1)
                            ed.Command("TEXTEDIT", nestedEntityResult.PickedPoint)
                        End Using
                        Exit Do
                End Select
            End If
            Select Case entity.GetDBObjectType
                Case "AttributeReference"
                    Dim attribute = entity.CastAsAttributeReference
                    Dim dialog = New InputBoxDialog("Attribute".Translate(attribute.Tag), attribute.TextString)
                    If Not dialog.DialogResult = DialogResult.OK Then Continue Do

                    Dim textToChange = New Dictionary(Of ObjectId, String) From {{entityId, dialog.InputText}}
                    TextHelper.ChangeTextStrings(document, textToChange)
                    'voor als dit attribuut onderdeel is van een kader,
                    'wordt de wijziging direct doorgevoerd i.p.v. na afsluiten commando
                    document.DocumentEvents.ProcessFrameChanges()
                Case Else
                    If hasAttributes Then ed.Command("DDATTE", nestedEntityResult.PickedPoint)
            End Select
        Loop
    End Sub

    ''' <summary>
    ''' Allows the user to copy text from a textobjects to other textobjects.
    ''' <para>Note: If the first text is an attributereference, the text will be copied to attributereferences with the same tag.</para>
    ''' </summary>
    ''' <param name="document">The present document.</param>
    Public Shared Sub Copy(document As Document)
        Dim db = document.Database
        Dim ed = document.Editor
        Dim textToChange = New Dictionary(Of ObjectId, String)
        Dim tag = ""
        Dim sourceText = "$$$"
        Dim promptNestedEntityOptions = New PromptNestedEntityOptions("Sel TextOrAttr:".Translate) With {.AllowNone = True}
        'selecteer eerste tekst of attribuut
        Dim nestedEntityResult = ed.GetNestedEntity(promptNestedEntityOptions)
        If Not nestedEntityResult.Status = PromptStatus.OK Then Exit Sub

        Using tr = db.TransactionManager.StartTransaction
            Dim entity = tr.GetEntity(nestedEntityResult.ObjectId)
            Select Case entity.GetDBObjectType
                Case "AttributeReference"
                    Dim attribute = entity.CastAsAttributeReference
                    tag = attribute.Tag
                    sourceText = attribute.TextString
                Case "DBText"
                    Dim btr = tr.GetBlockTableRecord(entity.OwnerId)
                    Select Case btr.Name.ToUpper
                        Case "*MODEL_SPACE", "*PAPER_SPACE" : sourceText = entity.CastAsDBText.TextString
                    End Select
            End Select
            If Not sourceText = "$$$" Then
                'selecteer teksten of attributen
                Dim promptSelectionOptions = New PromptSelectionOptions()
                Select Case tag = ""
                    Case True
                        promptSelectionOptions.MessageForAdding = "Sel Texts:".Translate
                        promptSelectionOptions.MessageForRemoval = "Rem Texts:".Translate
                    Case Else
                        promptSelectionOptions.MessageForAdding = "Sel Attributes:".Translate(tag)
                        promptSelectionOptions.MessageForRemoval = "Rem Attributes:".Translate
                End Select
                Dim selectionResult = ed.GetSelection(promptSelectionOptions)
                If selectionResult.Status = PromptStatus.OK Then
                    Dim symbolData = ReferenceHelper.GetReferenceData(db, New ObjectIdCollection(selectionResult.Value.GetObjectIds))
                    For Each entityId In selectionResult.Value.GetObjectIds
                        Dim row = symbolData.Rows.Find(entityId)
                        Select Case True
                            Case tag = "" And tr.GetEntity(entityId).GetDBObjectType = "DBText" : textToChange.TryAdd(entityId, sourceText)
                            Case tag = ""
                            Case row?.HasAttribute(tag) : textToChange.TryAdd(row.GetAttributeId(tag), sourceText)
                        End Select
                    Next
                End If
            End If
            TextHelper.ChangeTextStrings(document, textToChange)
            tr.Commit()
        End Using
    End Sub

    ''' <summary>
    ''' Allows the user to select two texts to be joined.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    Public Shared Sub Join(document As Document)
        Edit(document, SwapsOrJoins.Join)
    End Sub

    ''' <summary>
    ''' Allows the user to select two texts to be swapped.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    Public Shared Sub Swap(document As Document)
        Edit(document, SwapsOrJoins.Swap)
    End Sub

    ''' <summary>
    ''' Allows the user to select texts to add extra characters after.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    Public Shared Sub AddAfter(document As Document)
        Edit(document, EditModes.AddAfter)
    End Sub

    ''' <summary>
    ''' Allows the user to select texts to add extra characters before.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    Public Shared Sub AddBefore(document As Document)
        Edit(document, EditModes.AddBefore)
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="document">The present document.</param>
    Public Shared Sub AddNumber(document As Document)
        Edit(document, EditModes.AddNumber)
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="document">The present document.</param>
    Public Shared Sub ChangeMultiple(document As Document)
        Edit(document, EditModes.ChangeMultiple)
    End Sub

    ''' <summary>
    ''' Allows the user to select texts to make them lowercase (starting with a capital letter).
    ''' </summary>
    ''' <param name="document">The present document.</param>
    Public Shared Sub ToLower(document As Document)
        Edit(document, EditModes.ToLower)
    End Sub

    ''' <summary>
    ''' Allows the user to select texts to make them uppercase.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    Public Shared Sub ToUpper(document As Document)
        Edit(document, EditModes.ToUpper)
    End Sub

    'private subs

    ''' <summary>
    ''' The method by which the user can swap or join texts. 
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="swapOrJoin">The swap or join mode.</param>
    Private Shared Sub Edit(document As Document, swapOrJoin As SwapsOrJoins)
        Dim db = document.Database
        Dim ed = document.Editor
        Dim rollBackStack = New Stack(Of RollBackModel)   'to rollback steps when Undo is used within the method.
        Dim textObjects = New Dictionary(Of ObjectId, String)
        Dim promptNestedEntityOptions = New PromptNestedEntityOptions("") With {.AllowNone = True}
        promptNestedEntityOptions.Keywords.Add("Undo")
        Do
            promptNestedEntityOptions.Message = If(textObjects.Count = 0, "Sel First Text:", "Sel Second Text:").Translate
            Dim nestedEntityResult = ed.GetNestedEntity(promptNestedEntityOptions)
            If nestedEntityResult.Status = PromptStatus.Keyword AndAlso nestedEntityResult.StringResult = "Undo" Then
                If rollBackStack.Count < 1 Then Beep() : Continue Do

                Dim rollBackSession = rollBackStack.Pop
                TextHelper.ChangeTextStrings(document, rollBackSession.Strings)
                If NotNothing(rollBackSession.ObjectId) Then EntityHelper.Show(db, rollBackSession.ObjectId)
                textObjects = New Dictionary(Of ObjectId, String)
                Continue Do
            End If

            If Not nestedEntityResult.Status = PromptStatus.OK Then Exit Do

            Dim entityId = nestedEntityResult.ObjectId
            If textObjects.ContainsKey(entityId) Then Continue Do

            Dim lastWasDBText As Boolean
            Using tr = db.TransactionManager.StartTransaction
                Dim entity = tr.GetEntity(entityId)

                Select Case entity.GetDBObjectType
                    Case "AttributeReference"
                        lastWasDBText = False
                        textObjects.TryAdd(entityId, entity.CastAsAttributeReference.TextString)
                    Case "DBText"
                        lastWasDBText = True
                        If tr.EntityIsInModelOrPaperspace(entity) Then textObjects.TryAdd(entityId, entity.CastAsDBText.TextString)
                End Select
                tr.Commit()
            End Using
            If Not textObjects.Count = 2 Then Continue Do

            Dim textToChange = New Dictionary(Of ObjectId, String)
            Select Case swapOrJoin
                Case SwapsOrJoins.Swap
                    textToChange.TryAdd(textObjects.Keys(0), textObjects.Values(1))
                    textToChange.TryAdd(textObjects.Keys(1), textObjects.Values(0))
                    rollBackStack.Push(New RollBackModel(textObjects))
                Case SwapsOrJoins.Join
                    textToChange.TryAdd(textObjects.Keys(0), "{0} {1}".Compose(textObjects.Values(0), textObjects.Values(1)))
                    Select Case lastWasDBText
                        Case True : EntityHelper.Hide(db, textObjects.Keys(1))
                        Case Else : textToChange.TryAdd(textObjects.Keys(1), "")
                    End Select
                    rollBackStack.Push(New RollBackModel(textObjects, If(lastWasDBText, textObjects.Keys(1), Nothing)))
            End Select
            TextHelper.ChangeTextStrings(document, textToChange)
            textObjects = New Dictionary(Of ObjectId, String)
        Loop
        Do While rollBackStack.Count > 0
            Dim rollBackSession = rollBackStack.Pop
            If NotNothing(rollBackSession.ObjectId) Then EntityHelper.Delete(document, rollBackSession.ObjectId)
        Loop
    End Sub

    ''' <summary>
    ''' The method by which the user can edit texts. 
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="editMode">The editing mode.</param>
    Private Shared Sub Edit(document As Document, editMode As EditModes)
        Dim db = document.Database
        Dim ed = document.Editor
        Dim textObjects = New Dictionary(Of ObjectId, String)
        Dim textIds = SelectMethods.GetSelectionOfDBtexts(document)
        Using tr = db.TransactionManager.StartTransaction
            Select Case textIds.Count > 0
                Case True
                    For Each textId In textIds.ToArray
                        textObjects.TryAdd(textId, tr.GetDBText(textId).TextString)
                    Next
                Case Else
                    Dim promptNestedEntityOptions = New PromptNestedEntityOptions("Sel Attr:".Translate) With {.AllowNone = True}
                    Dim entity As Entity
                    Do
                        Dim nestedEntityResult = ed.GetNestedEntity(promptNestedEntityOptions)
                        If Not nestedEntityResult.Status = PromptStatus.OK Then Exit Sub

                        entity = tr.GetEntity(nestedEntityResult.ObjectId)
                        If entity.GetDBObjectType = "AttributeReference" Then Exit Do
                    Loop
                    Dim attribute = entity.CastAsAttributeReference
                    Dim reference = tr.GetBlockReference(attribute.OwnerId)
                    reference.Highlight()
                    Dim tag = attribute.Tag
                    textObjects.TryAdd(attribute.ObjectId, attribute.TextString)

                    Dim referenceIds = SelectMethods.GetSelectionOfReferences(document)
                    Dim symbolData = ReferenceHelper.GetReferenceData(db, referenceIds)
                    For Each symbolRow In symbolData.Select
                        If Not symbolRow.HasAttribute(tag) Then Continue For

                        textObjects.TryAdd(symbolRow.GetAttributeId(tag), symbolRow.GetString(tag))
                    Next
                    reference.Unhighlight()
            End Select
            tr.Commit()
        End Using
        If textObjects.Count = 0 Then Exit Sub

        Dim prompt = ""
        Select Case editMode
            Case EditModes.AddAfter : prompt = "AddAfter"
            Case EditModes.AddBefore : prompt = "AddBefore"
            Case EditModes.AddNumber : prompt = "AddNumber"
            Case EditModes.ChangeMultiple : prompt = "ChangeMultiple"
        End Select
        Dim value = ""
        If Not prompt = "" Then
            Dim dialog = New InputBoxDialog(prompt.Translate, "")
            If dialog.InputText = "" Then Exit Sub

            value = dialog.InputText
        End If
        Dim textToChange = New Dictionary(Of ObjectId, String)
        Dim result = ""
        For Each pair In textObjects
            Select Case editMode
                Case EditModes.AddAfter : result = "{0}{1}".Compose(pair.Value, value)
                Case EditModes.AddBefore : result = "{0}{1}".Compose(value, pair.Value)
                Case EditModes.AddNumber : result = pair.Value.AddNumber(value.ToInteger)
                Case EditModes.ChangeMultiple : result = value
                Case EditModes.ToLower : result = "{0}{1}".Compose(pair.Value.LeftString(1).ToUpper, pair.Value.MidString(2).ToLower)
                Case EditModes.ToUpper : result = pair.Value.ToUpper
            End Select
            textToChange.TryAdd(pair.Key, result)
        Next
        TextHelper.ChangeTextStrings(document, textToChange)
    End Sub

    'private enums

    ''' <summary>
    ''' An enum to determine the editing mode.
    ''' </summary>
    Private Enum EditModes
        AddAfter
        AddBefore
        AddNumber
        ChangeMultiple
        ToLower
        ToUpper
    End Enum

    ''' <summary>
    ''' An enum to determine the swap or join mode.
    ''' </summary>
    Private Enum SwapsOrJoins
        Join
        Swap
    End Enum

End Class
