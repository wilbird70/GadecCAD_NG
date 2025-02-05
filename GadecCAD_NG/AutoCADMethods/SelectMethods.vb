'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.EditorInput

''' <summary>
''' Provides methods for the user to select drawing objects.
''' </summary>
Public Class SelectMethods

    ''' <summary>
    ''' Allows the user to select only blockreferences.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="definitionNames">A list of blocknames to be allowed in the selection.</param>
    ''' <returns>A list of objectids of the selected objects.</returns>
    Public Shared Function GetSelectionOfReferences(document As Document, ParamArray definitionNames As String()) As ObjectIdCollection
        Dim db = document.Database
        Dim ed = document.Editor
        Dim filterList = {New TypedValue(DxfCode.Start, "INSERT")}.ToList
        If definitionNames.Count > 0 Then
            definitionNames = DefinitionHelper.AddAnonymousNames(db, definitionNames)
            filterList.Add(New TypedValue(DxfCode.BlockName, String.Join(",", definitionNames)))
        End If
        Dim filter = New SelectionFilter(filterList.ToArray)
        Dim promptSelectionOptions = New PromptSelectionOptions With {.MessageForAdding = "Sel Blocks:".Translate, .MessageForRemoval = "Rem Blocks:".Translate}
        Dim selectionResult = ed.GetSelection(promptSelectionOptions, filter)
        If Not selectionResult.Status = PromptStatus.OK Then Return New ObjectIdCollection

        Return New ObjectIdCollection(selectionResult.Value.GetObjectIds)
    End Function

    ''' <summary>
    ''' Allows the user to select only texts (DBTEXT).
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <returns>A list of objectids of the selected objects.</returns>
    Public Shared Function GetSelectionOfDBtexts(document As Document) As ObjectIdCollection
        Dim ed = document.Editor
        Dim filterList = {New TypedValue(DxfCode.Start, "TEXT")}
        Dim filter = New SelectionFilter(filterList)
        Dim promptSelectionOptions = New PromptSelectionOptions With {.MessageForAdding = "Sel Texts:".Translate, .MessageForRemoval = "Rem Texts:".Translate}
        Dim selectionResult = ed.GetSelection(promptSelectionOptions, filter)
        If Not selectionResult.Status = PromptStatus.OK Then Return New ObjectIdCollection

        Return New ObjectIdCollection(selectionResult.Value.GetObjectIds)
    End Function

    ''' <summary>
    ''' Allows the user to select only curves (eg. lines, arcs and polylines).
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <returns>A list of objectids of the selected objects.</returns>
    Public Shared Function GetSelectionOfCurves(document As Document) As ObjectIdCollection
        Dim db = document.Database
        Dim ed = document.Editor
        Dim filterList = {
            New TypedValue(DxfCode.Operator, "<OR"),
            New TypedValue(DxfCode.Start, "ARC,ELLIPSE,LINE,LWPOLYLINE"),
            New TypedValue(DxfCode.Operator, "<AND"),
            New TypedValue(DxfCode.Start, "SPLINE"),
            New TypedValue(DxfCode.Operator, "&"),
            New TypedValue(DxfCode.Int16, 8),
            New TypedValue(DxfCode.Operator, "AND>"),
            New TypedValue(DxfCode.Operator, "OR>")
        }
        Dim filter = New SelectionFilter(filterList)
        Dim promptSelectionOptions = New PromptSelectionOptions With {.MessageForAdding = "Sel Lines:".Translate, .MessageForRemoval = "Rem Lines:".Translate}
        Dim selectionResult = ed.GetSelection(promptSelectionOptions, filter)
        If Not selectionResult.Status = PromptStatus.OK Then Return New ObjectIdCollection

        Return New ObjectIdCollection(selectionResult.Value.GetObjectIds)
    End Function

End Class
