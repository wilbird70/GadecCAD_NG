'Gadec Engineerings Software (c) 2022
Imports System.Data
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.EditorInput
Imports Autodesk.AutoCAD.Geometry
Imports GadecCAD_NG.Extensions

''' <summary>
''' <para><see cref="SymbolEncoder"/> allows the user to select blockreferences to encode their attributes.</para>
''' </summary>
Public Class SymbolEncoder
    ''' <summary>
    ''' The present document.
    ''' </summary>
    Private ReadOnly _document As Document
    ''' <summary>
    ''' An instance of <see cref="SymbolCodeModel"/> with detailed information about encoding symbols.
    ''' </summary>
    Private _code As SymbolCodeModel

    ''' <summary>
    ''' Initializes a new instance of <see cref="SymbolEncoder"/>.
    ''' <para><see cref="SymbolEncoder"/> allows the user to select blockreferences to encode their attributes.</para>
    ''' </summary>
    ''' <param name="document">The present document.</param>
    Public Sub New(document As Document)
        _document = document
    End Sub

    ''' <summary>
    ''' Runs the encode command.
    ''' <para>Codes consist of fixed values for most attributes and a attribute with a count number (eg 'P01' 'L01-' '001').</para>
    ''' <para>See <see cref="SymbolCodeModel"/> for the different properties.</para>
    ''' </summary>
    ''' <param name="code">An instance of <see cref="SymbolCodeModel"/> with detailed information about encoding symbols.</param>
    Public Sub Start(code As SymbolCodeModel)
        _code = code
        Select Case _code.CounterTag = ""
            Case True : SelectionCoding()
            Case Else : CounterCoding()
        End Select
    End Sub

    'Private subs

    ''' <summary>
    ''' Codes attributes of AutoCAD blockreferences without a counting number.
    ''' </summary>
    Private Sub SelectionCoding()
        Dim ed = _document.Editor
        Dim promptSelectionOptions = New PromptSelectionOptions()
        Dim selectionResult = ed.GetSelection(promptSelectionOptions)
        If selectionResult.Status = PromptStatus.OK Then
            Dim objectIds = New ObjectIdCollection(selectionResult.Value.GetObjectIds)
            Dim symbolData = ReferenceHelper.GetReferenceData(_document.Database, objectIds)
            Dim codeData = _code.Texts.ToIniDictionary
            For Each row In symbolData.Select
                Dim textToChange = CodeSymbol(row, codeData)
                TextHelper.ChangeTextStrings(_document, textToChange)
            Next
        End If
        Autodesk.AutoCAD.Internal.Utils.PostCommandPrompt()
    End Sub

    ''' <summary>
    ''' Codes attributes of AutoCAD blockreferences with a counting number.
    ''' </summary>
    Private Sub CounterCoding()
        Dim db = _document.Database
        Dim ed = _document.Editor
        Dim codeData = _code.Texts.ToIniDictionary
        codeData.TryAdd(_code.LocationTag, "")
        Dim locationPrefix = codeData(_code.LocationTag)
        Dim symbolIds = New ObjectIdCollection              'to bring symbols to front when closing method.
        Dim locationIds = New List(Of ObjectId)             'to make location attribute invisible when closing method.
        Dim rollBackStack = New Stack(Of RollBackModel)     'to rollback steps when Undo is used within the method.
        Dim lastPoint As Point3d

        Dim promptEntityOptions = New PromptEntityOptions("X")
        promptEntityOptions.Keywords.Add("Undo")
        promptEntityOptions.Keywords.Add("Remove")
        promptEntityOptions.Keywords.Add("LLoopline")
        Do
            Select Case codeData(_code.LocationTag) = ""
                Case True : promptEntityOptions.Message = "Sel Element:".Translate(codeData(_code.CounterTag))
                Case Else : promptEntityOptions.Message = "Sel Element:".Translate("{0}, {1}".Compose(codeData(_code.CounterTag), codeData(_code.LocationTag)))
            End Select
            Dim selectionResult = ed.GetEntity(promptEntityOptions)
            If selectionResult.Status = PromptStatus.Keyword Then
                Select Case selectionResult.StringResult
                    Case "Remove" : If codeData.ContainsKey(_code.LocationTag) Then codeData(_code.LocationTag) = locationPrefix
                    Case "LLoopline" : _code.drawLoopLine = Not _code.drawLoopLine
                    Case "Undo"
                        If rollBackStack.Count < 1 Then Beep() : Continue Do

                        Dim rollBackSession = rollBackStack.Pop
                        TextHelper.ChangeTextStrings(_document, rollBackSession.Strings)
                        codeData(_code.CounterTag) = codeData(_code.CounterTag).AddNumber(-_code.CounterStep * rollBackSession.Number)
                        If rollBackStack.Count > 0 Then EntityHelper.Delete(_document, rollBackSession.ObjectId)
                        lastPoint = rollBackSession.Point3d
                End Select
                Continue Do
            End If

            If Not selectionResult.Status = PromptStatus.OK Then Exit Do

            If Not selectionResult.ObjectId.ObjectClass.DxfName.ToLower = "insert" Then
                Dim text = GetTextString(selectionResult.ObjectId)
                If Not text = "" Then codeData(_code.LocationTag) = ConcatenateStringToMax19Chars(locationPrefix, text)
                Continue Do
            End If

            Dim textToChange = New List(Of KeyValuePair(Of ObjectId, String))
            Dim setVisibleLocationIds = New ObjectIdCollection
            Dim numberOfSymbols = 0
            Dim groupedIds = GroupHelper.GetGroupedEntityIds(db, selectionResult.ObjectId)
            If groupedIds.Count = 0 Then groupedIds.Add(selectionResult.ObjectId)
            Dim referenceRows = ReferenceHelper.GetReferenceData(db, groupedIds).Rows.ToArray
            Dim newPoint = referenceRows.First.GetPoint3d("Position")
            Dim startNumber = If(_code.CounterStep = -1, referenceRows.Count - 1, 0)
            Dim endNumber = If(_code.CounterStep = -1, 0, referenceRows.Count - 1)
            For i = startNumber To endNumber Step _code.CounterStep
                Dim row = referenceRows(i)
                If Not symbolIds.Contains(row.GetValue("ObjectID")) Then symbolIds.Add(row.GetValue("ObjectID"))
                Dim counterId = New ObjectId
                Dim locationId = New ObjectId
                Dim coding = CodeSymbol(row, codeData, counterId, locationId)
                If Not counterId.IsNull Then
                    codeData(_code.CounterTag) = codeData(_code.CounterTag).AddNumber(_code.CounterStep)
                    textToChange.AddRange(coding)
                    numberOfSymbols += 1
                    If Not locationId.IsNull Then setVisibleLocationIds.Add(locationId)
                End If
            Next
            If textToChange.Count = 0 Then
                Dim promptNestedEntityOptions = New PromptNestedEntityOptions("Auto") With {.NonInteractivePickPoint = selectionResult.PickedPoint, .UseNonInteractivePickPoint = True}
                Dim nestedEntityResult = ed.GetNestedEntity(promptNestedEntityOptions)
                If nestedEntityResult.Status = PromptStatus.OK Then
                    Dim text = GetTextString(nestedEntityResult.ObjectId)
                    If Not text = "" Then codeData(_code.LocationTag) = ConcatenateStringToMax19Chars(locationPrefix, text)
                End If
                Continue Do
            End If

            Dim previousTexts = TextHelper.ChangeTextStrings(_document, textToChange.ToArray)
            AttributeHelper.SetVisibility(db, setVisibleLocationIds, True)
            locationIds.AddRange(setVisibleLocationIds.ToArray)
            Dim lineId = If(_code.drawLoopLine And rollBackStack.Count > 0, LoopLineHelper.CreateLoopLine(_document, lastPoint, newPoint, _code.LoopLineLayerId), ObjectId.Null)
            rollBackStack.Push(New RollBackModel(previousTexts, lineId, lastPoint, numberOfSymbols))
            lastPoint = newPoint
        Loop
        DrawOrderHelper.BringToFront(_document, symbolIds)
        If locationIds.Count > 0 Then AttributeHelper.SetVisibility(db, New ObjectIdCollection(locationIds.ToArray), False)
    End Sub

    'Private functions

    ''' <summary>
    ''' Merge two strings and maximize the number of characters to 19.
    ''' </summary>
    ''' <param name="text">The text to start with.</param>
    ''' <param name="append">The text to append.</param>
    ''' <returns>The merged string.</returns>
    Private Function ConcatenateStringToMax19Chars(text As String, append As String)
        append = "{0}{1}".Compose(text, append)
        If append.Length > 19 Then Beep()
        Return append.LeftString(19)
    End Function

    ''' <summary>
    ''' Gets the textstring of an AutoCAD textobject (DBText) or attribute.
    ''' <para>Returns empty string if entity is not a textobject or attribute.</para>
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="entityId">The objectid of the entity.</param>
    ''' <returns>The text string.</returns>
    Private Function GetTextString(entityId As ObjectId) As String
        Using tr = _document.Database.TransactionManager.StartTransaction
            Dim entity = tr.GetEntity(entityId)
            Select Case entity.GetDBObjectType
                Case "DBText" : Return entity.CastAsDBText.TextString
                Case "AttributeReference" : Return entity.CastAsAttributeReference.TextString
                Case Else : Return ""
            End Select
            tr.Commit()
        End Using
    End Function

    ''' <summary>
    ''' Codes attributes of an AutoCAD blockreference with a counting number.
    ''' </summary>
    ''' <param name="symbolRow">A record from the database with symboldata.</param>
    ''' <param name="counterId">Returns the objectid of an attribute with the countertag.</param>
    ''' <param name="locationId">Returns the objectid of an attribute with the locationtag.</param>
    ''' <returns>A dictionary containing textchanges for the attributes.</returns>
    Private Function CodeSymbol(symbolRow As DataRow, codeData As Dictionary(Of String, String), Optional ByRef counterId As ObjectId = Nothing, Optional ByRef locationId As ObjectId = Nothing) As Dictionary(Of ObjectId, String)
        counterId = New ObjectId
        locationId = New ObjectId
        Dim textToChange = New Dictionary(Of ObjectId, String)
        Dim attributesInOrder = symbolRow.GetAttributeColumns.Where(Function(column) symbolRow.HasAttribute(column)).Select(Function(column) column)
        For Each pair In codeData
            Dim key = ""
            Select Case True
                Case Not pair.Key.StartsWith("#") : key = pair.Key
                Case pair.Key.MidString(2).ToInteger - 1 < attributesInOrder.Count : key = attributesInOrder(pair.Key.MidString(2).ToInteger - 1)
            End Select
            If symbolRow.HasAttribute(key) Then
                Dim attrubuteId = symbolRow.GetAttributeId(key)
                textToChange.TryAdd(attrubuteId, pair.Value)
                Select Case pair.Key
                    Case _code.CounterTag : counterId = attrubuteId
                    Case _code.LocationTag : locationId = attrubuteId
                End Select
            End If
        Next
        If symbolRow.HasValue("DOT") Then textToChange.TryAdd(symbolRow.GetAttributeId("DOT"), "/")
        Return textToChange
    End Function

End Class
