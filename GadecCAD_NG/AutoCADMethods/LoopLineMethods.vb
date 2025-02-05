'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.EditorInput
Imports Autodesk.AutoCAD.Geometry

''' <summary>
''' Provides methods to draw a lines between insertion points of blockreferences.
''' </summary>
Public Class LoopLineMethods

    'subs

    ''' <summary>
    ''' Allows the user to select blockreferences for drawing a polyline (3 vertices) between them.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="type">'LL' for loopline and 'RL' for remoteline.</param>
    ''' <param name="layerId">The objectid of the layer to be used.</param>
    Public Shared Sub DrawLoopLine(document As Document, layerId As ObjectId)
        Dim db = document.Database
        Dim ed = document.Editor
        Dim symbolIds = New ObjectIdCollection              'to bring symbols to front when closing method.
        Dim rollBackStack = New Stack(Of RollBackModel)     'to rollback steps when Undo is used within the method.
        Dim lastPoint As Nullable(Of Point3d) = Nothing
        Dim promptEntityOptions = New PromptEntityOptions("X")
        promptEntityOptions.Keywords.Add("Undo")
        Do
            Select Case IsNothing(lastPoint)
                Case True : promptEntityOptions.Message = ("Sel First Block").Translate
                Case Else : promptEntityOptions.Message = ("Sel Next Block").Translate
            End Select
            Dim selectionResult = ed.GetEntity(promptEntityOptions)
            If selectionResult.Status = PromptStatus.Keyword AndAlso selectionResult.StringResult = "Undo" Then
                If rollBackStack.Count < 1 Then Beep() : Continue Do

                Dim rollBackSession = rollBackStack.Pop
                EntityHelper.Delete(document, rollBackSession.ObjectId)
                lastPoint = rollBackSession.Point3d
                Continue Do
            End If

            If Not selectionResult.Status = PromptStatus.OK Then Exit Do

            Dim symbolData = ReferenceHelper.GetReferenceData(db, selectionResult.ObjectId)
            If IsNothing(symbolData) Then Continue Do

            If Not symbolIds.Contains(selectionResult.ObjectId) Then symbolIds.Add(selectionResult.ObjectId)
            Dim selectedInsertionPoint = symbolData.GetValue("Position")
            If IsNothing(lastPoint) Then lastPoint = selectedInsertionPoint : Continue Do

            Dim lineId = LoopLineHelper.CreateLoopLine(document, lastPoint, selectedInsertionPoint, layerId)
            rollBackStack.Push(New RollBackModel(lineId, lastPoint))
            lastPoint = selectedInsertionPoint
        Loop
        DrawOrderHelper.BringToFront(document, symbolIds)
    End Sub

    ''' <summary>
    ''' Allows the user to select blockreferences for drawing a dashed line between them.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="type">'LL' for loopline and 'RL' for remoteline.</param>
    ''' <param name="layerId">The objectid of the layer to be used.</param>
    Public Shared Sub DrawRemoteLine(document As Document, layerId As ObjectId)
        Dim db = document.Database
        Dim ed = document.Editor
        Dim symbolIds = New ObjectIdCollection          'to bring symbols to front when closing method.
        Dim rollBackStack = New Stack(Of RollBackModel) 'to rollback steps when Undo is used within the method.
        Dim lastPoint As Nullable(Of Point3d) = Nothing
        Dim promptEntityOptions = New PromptEntityOptions("X")
        promptEntityOptions.Keywords.Add("Undo")
        Do
            Select Case IsNothing(lastPoint)
                Case True : promptEntityOptions.Message = ("Sel First Block").Translate
                Case Else : promptEntityOptions.Message = ("Sel Next Block").Translate
            End Select
            Dim selectionResult = ed.GetEntity(promptEntityOptions)
            If selectionResult.Status = PromptStatus.Keyword AndAlso selectionResult.StringResult = "Undo" Then
                If rollBackStack.Count < 1 Then Beep() : Continue Do

                Dim rollBackSession = rollBackStack.Pop
                EntityHelper.Delete(document, rollBackSession.ObjectId)
                lastPoint = Nothing
                Continue Do
            End If

            If Not selectionResult.Status = PromptStatus.OK Then Exit Do

            Dim symbolData = ReferenceHelper.GetReferenceData(db, selectionResult.ObjectId)
            If IsNothing(symbolData) Then Continue Do

            If Not symbolIds.Contains(selectionResult.ObjectId) Then symbolIds.Add(selectionResult.ObjectId)
            Dim selectedInsertionPoint = symbolData.GetValue("Position")
            If IsNothing(lastPoint) Then lastPoint = selectedInsertionPoint : Continue Do

            Dim lineId = LoopLineHelper.CreateRemoteLine(document, lastPoint, selectedInsertionPoint, layerId)
            rollBackStack.Push(New RollBackModel(lineId))
            lastPoint = Nothing
        Loop
        DrawOrderHelper.BringToFront(document, symbolIds)
    End Sub

End Class

