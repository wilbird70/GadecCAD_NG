'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.EditorInput
Imports Autodesk.AutoCAD.Geometry
Imports GadecCAD_NG.Extensions

''' <summary>
''' Provides methods for finding frames and their headers.
''' </summary>
Public Class FrameFindHelper

    'subs

    ''' <summary>
    ''' Finds the all the frames in the specified files and register them in the drawings xrecords.
    ''' </summary>
    ''' <param name="files">The list of fullnames of the files.</param>
    Public Shared Sub SearchThroughDocuments(files As String())
        Dim documents = DocumentsHelper.GetDocumentNames()
        Dim documentsToClose = New List(Of Document)
        If files.Count > 5 Then Progressbar = New ProgressShow("Starting...".Translate, files.Count)
        For Each file In files
            If Progressbar?.CancelPressed Then Exit For

            Progressbar?.PerformStep("{0}{1}".Compose("Opening...".Translate, FileSystemHelper.LimitDisplayLengthFileName(file, 60)))
            If documents.Contains(file) Then Continue For

            Dim doc = DocumentsHelper.Open(file, False)
            If IsNothing(doc) Then Continue For

            documentsToClose.Add(doc)
            Progressbar?.SetText("{0}{1}".Compose("Reading...".Translate, FileSystemHelper.LimitDisplayLengthFileName(file, 60)))
            Dim frameData = doc.FrameData
            Dim frameIdCollections = FrameHelper.GetFrameIdCollections(doc.FrameData)
            XRecordObjectIdsHelper.Update(doc, "{Company}".Compose, "FrameWorkIDs", frameIdCollections)
            If documentsToClose.Count < 10 Then Continue For

            DocumentsHelper.Close(documentsToClose.ToArray, True)
            documentsToClose.Clear()
        Next
        DocumentsHelper.Close(documentsToClose.ToArray, True)
        Progressbar?.Dispose()
        Progressbar = Nothing
    End Sub

    'functions

    ''' <summary>
    ''' Finds the frames in all the layout-tabs of a <see cref="Document"/>.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <returns><see cref="ObjectIdCollection"/> with objectsids of all the frames.</returns>
    Public Shared Function AllFrames(document As Document) As ObjectIdCollection
        Dim ed = document.Editor
        Dim frameFilterData = DataSetHelper.LoadFromXml("{Support}\SetFramesInfo.xml".Compose).GetTable("FrameFilter", "Name")
        If IsNothing(frameFilterData) Then Return New ObjectIdCollection

        Dim frames = String.Join(",", frameFilterData.GetStringsFromColumn("Name"))
        Dim filterList = {New TypedValue(DxfCode.Start, "INSERT"), New TypedValue(DxfCode.BlockName, frames)}
        Dim filter = New SelectionFilter(filterList)
        Try
            Dim selectionResult = ed.SelectAll(filter)
            If Not selectionResult.Status = PromptStatus.OK Then Return New ObjectIdCollection

            Return New ObjectIdCollection(selectionResult.Value.GetObjectIds)
        Catch ex As Autodesk.AutoCAD.Runtime.Exception
            If ex.ErrorStatus = Autodesk.AutoCAD.Runtime.ErrorStatus.NotApplicable Then Return New ObjectIdCollection
            ex.Rethrow
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' Finds the headers within the extends of a frame.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="layoutName">The name of the <see cref="Layout"/> where the frame is located.</param>
    ''' <param name="frameExtents">The extends of the frame.</param>
    ''' <param name="scalefactor">The scalefactor of the frame.</param>
    ''' <returns></returns>
    Public Shared Function HeadersByFrame(document As Document, layoutName As String, frameExtents As Extents3d, scalefactor As Double) As ObjectIdCollection
        Dim ed = document.Editor
        Dim headerFilterData = DataSetHelper.LoadFromXml("{Support}\SetFramesInfo.xml".Compose).GetTable("HeaderFilter", "Name")
        If IsNothing(headerFilterData) Then Return New ObjectIdCollection

        Dim headers = String.Join(",", headerFilterData.GetStringsFromColumn("Name"))
        Dim minPoint = New Point3d(frameExtents.MinPoint.X - scalefactor, frameExtents.MinPoint.Y - scalefactor, 0)
        Dim maxPoint = New Point3d(frameExtents.MaxPoint.X + scalefactor, frameExtents.MaxPoint.Y + scalefactor, 0)
        Dim filterList = {New TypedValue(DxfCode.LayoutName, layoutName), New TypedValue(DxfCode.Start, "INSERT"), New TypedValue(DxfCode.BlockName, headers),
            New TypedValue(DxfCode.Operator, "<AND"), New TypedValue(DxfCode.Operator, ">=,>=,*"), New TypedValue(DxfCode.XCoordinate, minPoint),
            New TypedValue(DxfCode.Operator, "<=,<=,*"), New TypedValue(DxfCode.XCoordinate, maxPoint), New TypedValue(DxfCode.Operator, "AND>")}
        Dim filter = New SelectionFilter(filterList)
        Try
            Dim selectionResult = ed.SelectAll(filter)
            If Not selectionResult.Status = PromptStatus.OK Then Return New ObjectIdCollection

            Return New ObjectIdCollection(selectionResult.Value.GetObjectIds)
        Catch ex As Autodesk.AutoCAD.Runtime.Exception
            If ex.ErrorStatus = Autodesk.AutoCAD.Runtime.ErrorStatus.NotApplicable Then Return New ObjectIdCollection
            ex.Rethrow
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' Finds the headers in the specicified layout of a <see cref="Document"/>.
    ''' <para>Headers can be used in customer frames, making them not visible in the frametab of the palette.</para>
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="layoutName">The name of the present <see cref="Layout"/>.</param>
    ''' <returns></returns>
    Public Shared Function FramelessHeaders(document As Document, layoutName As String) As ObjectIdCollection
        Dim ed = document.Editor
        Dim headerFilterData = DataSetHelper.LoadFromXml("{Support}\SetFramesInfo.xml".Compose).GetTable("HeaderFilter", "Name") 'not needed
        If IsNothing(headerFilterData) Then Return New ObjectIdCollection

        Dim headers = String.Join(",", headerFilterData.GetStringsFromColumn("Name"))
        Dim filterList = {New TypedValue(DxfCode.LayoutName, layoutName), New TypedValue(DxfCode.Start, "INSERT"), New TypedValue(DxfCode.BlockName, headers)}
        Dim filter = New SelectionFilter(filterList)
        Try
            Dim selectionResult = ed.SelectAll(filter)
            If Not selectionResult.Status = PromptStatus.OK Then Return New ObjectIdCollection

            Return New ObjectIdCollection(selectionResult.Value.GetObjectIds)
        Catch ex As Autodesk.AutoCAD.Runtime.Exception
            If ex.ErrorStatus = Autodesk.AutoCAD.Runtime.ErrorStatus.NotApplicable Then Return New ObjectIdCollection
            ex.Rethrow
            Return Nothing
        End Try
    End Function

End Class
