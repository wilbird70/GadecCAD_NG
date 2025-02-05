'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.EditorInput
Imports Autodesk.AutoCAD.Geometry

''' <summary>
''' Provides methods for selecting drawing objects.
''' </summary>
Public Class SelectHelper

    ''' <summary>
    ''' Gets all blockreferences within the modelspace.
    ''' </summary>
    ''' <param name="document"></param>
    ''' <param name="definitionNames">A list of blocknames to be allowed.</param>
    ''' <returns></returns>
    Public Shared Function GetAllReferencesInModelspace(document As Document, ParamArray definitionNames As String()) As ObjectIdCollection
        Dim db = document.Database
        Dim ed = document.Editor
        Dim filterList = {New TypedValue(DxfCode.LayoutName, "Model"), New TypedValue(DxfCode.Start, "INSERT")}.ToList
        If definitionNames.Count > 0 Then
            definitionNames = DefinitionHelper.AddAnonymousNames(db, definitionNames)
            filterList.Add(New TypedValue(DxfCode.BlockName, String.Join(",", definitionNames)))
        End If
        Dim filter = New SelectionFilter(filterList.ToArray)
        Dim selectionResult = ed.SelectAll(filter)
        If Not selectionResult.Status = PromptStatus.OK Then Return New ObjectIdCollection

        Return New ObjectIdCollection(selectionResult.Value.GetObjectIds)
    End Function

    ''' <summary>
    ''' Gets all blockreferences with the specified name in an area of a layout.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="layoutName">The name of the layout.</param>
    ''' <param name="area">The area.</param>
    ''' <param name="tolerance">The tolerance to take into account.</param>
    ''' <param name="blockName">The blockname.</param>
    ''' <returns></returns>
    Public Shared Function GetReferencesInArea(document As Document, layoutName As String, area As Extents3d, tolerance As Double, blockName As String) As ObjectIdCollection
        Dim ed = document.Editor
        Dim minPoint = New Point3d(area.MinPoint.X - tolerance, area.MinPoint.Y - tolerance, 0)
        Dim maxPoint = New Point3d(area.MaxPoint.X + tolerance, area.MaxPoint.Y + tolerance, 0)
        Dim filterList = {
            New TypedValue(DxfCode.LayoutName, layoutName),
            New TypedValue(DxfCode.Start, "INSERT"),
            New TypedValue(DxfCode.BlockName, blockName),
            New TypedValue(DxfCode.Operator, "<AND"),
            New TypedValue(DxfCode.Operator, ">=,>=,*"),
            New TypedValue(DxfCode.XCoordinate, minPoint),
            New TypedValue(DxfCode.Operator, "<=,<=,*"),
            New TypedValue(DxfCode.XCoordinate, maxPoint),
            New TypedValue(DxfCode.Operator, "AND>")
        }
        Dim filter = New SelectionFilter(filterList)
        Dim selectionResult = ed.SelectAll(filter)
        If Not selectionResult.Status = PromptStatus.OK Then Return New ObjectIdCollection

        Return New ObjectIdCollection(selectionResult.Value.GetObjectIds)
    End Function

End Class
