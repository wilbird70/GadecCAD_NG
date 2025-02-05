'Gadec Engineerings Software (c) 2022
Imports System.Data
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.EditorInput
Imports Autodesk.AutoCAD.Geometry
Imports GadecCAD_NG.Extensions

''' <summary>
''' <para><see cref="XlinksHandler"/> allows the user to check and modify the reference links.</para>
''' </summary>
Public Class XlinksHandler
    ''' <summary>
    ''' The present document.
    ''' </summary>
    Private ReadOnly _document As Document
    ''' <summary>
    ''' The present drawing.
    ''' </summary>
    Private ReadOnly _database As Database
    ''' <summary>
    ''' The present editor.
    ''' </summary>
    Private ReadOnly _editor As Editor
    ''' <summary>
    ''' A <see cref="Dictionary"/> with page numbers and page-extents.
    ''' </summary>
    Private ReadOnly _linkPages As Dictionary(Of String, Extents3d)

    ''' <summary>
    ''' Contains a stack of sessions to rollback if Undo is used within the method.
    ''' </summary>
    Private ReadOnly _rollBackStack As New Stack(Of RollBackModel)

    ''' <summary>
    ''' Initializes a new instance of <see cref="XlinksHandler"/>.
    ''' <para><see cref="XlinksHandler"/> allows the user to check and modify the reference links.</para>
    ''' </summary>
    ''' <param name="document">The present document.</param>
    Public Sub New(document As Document)
        _document = document
        _database = document.Database
        _editor = document.Editor
        _linkPages = GetXlinkPages()
    End Sub

    ''' <summary>
    ''' Runs the reference-links commmand. lc49g95tssuxen
    ''' </summary>
    Public Sub Start()
        Dim linkData = New Dictionary(Of ObjectId, DataRow)
        Dim promptEntityOptions = New PromptEntityOptions("XXX")
        promptEntityOptions.Keywords.Add("Undo")
        Do
            Using transient = New TransientHandler
                Dim links = GetXlinkBubbles()
                For Each value In links.Values
                    Dim transientpoints = AdjustLinkTexts(value)
                    If transientpoints.Count = 1 Then transient.ShowCircle(transientpoints(0), 10, True) : Continue For

                    For i = 0 To transientpoints.Count - 2
                        transient.ShowArc(transientpoints(i), transientpoints(i + 1), transientpoints.Count > 2)
                    Next
                Next
                promptEntityOptions.Message = If(linkData.Count = 0, "Sel First Block", "Sel Next Block").Translate
                Dim selectionResult = _editor.GetEntity(promptEntityOptions)
                If selectionResult.Status = PromptStatus.Keyword AndAlso selectionResult.StringResult = "Undo" Then
                    If _rollBackStack.Count < 1 Then Beep() : Continue Do

                    Dim rollBackSession = _rollBackStack.Pop
                    TextHelper.ChangeTextStrings(_document, rollBackSession.Strings)
                    Continue Do
                End If

                If Not selectionResult.Status = PromptStatus.OK Then Exit Do

                Dim referenceId = selectionResult.ObjectId
                If linkData.ContainsKey(referenceId) Then Continue Do

                Dim referenceData = ReferenceHelper.GetReferenceData(_database, referenceId)
                If Not referenceData?.GetString("BlockName") = "W_XLINKS" Then Continue Do

                linkData.Add(referenceId, referenceData)
                If Not linkData.Count = 2 Then Continue Do

                Dim session = New Dictionary(Of ObjectId, String)
                session.Add(linkData.Values(0).GetAttributeId("LINKCODE"), linkData.Values(0).GetString("LINKCODE"))
                session.Add(linkData.Values(1).GetAttributeId("LINKCODE"), linkData.Values(1).GetString("LINKCODE"))
                session.Add(linkData.Values(0).GetAttributeId("SY"), linkData.Values(0).GetString("SY"))
                session.Add(linkData.Values(1).GetAttributeId("SY"), linkData.Values(1).GetString("SY"))
                _rollBackStack.Push(New RollBackModel(session))

                Dim textToChange = New Dictionary(Of ObjectId, String)
                Dim linkCode = Randomizer.GetString(16)
                textToChange.TryAdd(linkData.Values(0).GetAttributeId("LINKCODE"), linkCode)
                textToChange.TryAdd(linkData.Values(1).GetAttributeId("LINKCODE"), linkCode)
                TextHelper.ChangeTextStrings(_document, textToChange)
                linkData = New Dictionary(Of ObjectId, DataRow)
            End Using
        Loop
    End Sub

    'private subs

    ''' <summary>
    ''' Sets a new link between two bubbles.
    ''' </summary>
    ''' <param name="objectIds">List of objectids of bubbles linked together.</param>
    Private Sub SetNewLinkCode(objectIds As ObjectIdCollection)
    End Sub

    ''' <summary>
    ''' Draws the connecting curves between linked bubbles.
    ''' </summary>
    ''' <param name="linkCode">If specified, a new link reference is created.</param>
    ''' <param name="objectIds">List of objectids of bubbles linked together.</param>
    Private Function AdjustLinkTexts(objectIds As ObjectIdCollection) As Point3dCollection
        Dim textToChange = New Dictionary(Of ObjectId, String)
        If objectIds.Count = 1 Then
            Dim row = ReferenceHelper.GetReferenceData(_database, objectIds(0))
            textToChange.TryAdd(row.GetAttributeId("FROM"), "")
            textToChange.TryAdd(row.GetAttributeId("TO"), "")
            TextHelper.ChangeTextStrings(_document, textToChange)
            Return New Point3dCollection({row.GetValue("Position")})
        End If

        Dim output = New Point3dCollection
        Dim linkCollection = GetLinkCollection(objectIds)
        Dim sortedKeys = linkCollection.Keys.ToSortedList
        Dim referenceIds = New ObjectIdCollection
        Dim referenceString = ""
        For i = 0 To sortedKeys.Count - 1
            Dim row = linkCollection(sortedKeys(i))
            If referenceString = "" AndAlso Not row.GetString("SY") = "#" Then referenceString = row.GetString("SY")
            referenceIds.Add(row.GetAttributeId("SY"))
            Select Case i
                Case 0
                    textToChange.TryAdd(row.GetAttributeId("FROM"), "")
                    textToChange.TryAdd(row.GetAttributeId("TO"), sortedKeys(i + 1).Replace(".0", "."))
                Case sortedKeys.Count - 1
                    textToChange.TryAdd(row.GetAttributeId("FROM"), sortedKeys(i - 1).Replace(".0", "."))
                    textToChange.TryAdd(row.GetAttributeId("TO"), "")
                Case Else
                    textToChange.TryAdd(row.GetAttributeId("FROM"), sortedKeys(i - 1).Replace(".0", "."))
                    textToChange.TryAdd(row.GetAttributeId("TO"), sortedKeys(i + 1).Replace(".0", "."))
            End Select
            output.Add(row.GetValue("Position"))
        Next
        For i = 0 To referenceIds.Count - 1
            textToChange.TryAdd(referenceIds(i), referenceString)
        Next
        TextHelper.ChangeTextStrings(_document, textToChange)
        Return output
    End Function

    'private functions

    ''' <summary>
    ''' Gets a list of page numbers and the extents of the linkpages.
    ''' </summary>
    ''' <returns>The <see cref="Dictionary"/> with page numbers and page-extents.</returns>
    Private Function GetXlinkPages() As Dictionary(Of String, Extents3d)
        Dim output = New Dictionary(Of String, Extents3d)
        Dim frameData = _document.FrameData
        For Each frameRow In frameData.Select
            Dim linkPageIds = SelectHelper.GetReferencesInArea(_document, frameRow.GetString("LayoutName"), frameRow.GetValue("BlockExtents"), frameRow.GetValue("ScaleFactor"), "W_XLINES")
            If linkPageIds.Count = 0 Then Continue For

            Dim pageRow = ReferenceHelper.GetReferenceData(_database, linkPageIds(0))
            Dim pageNumber = If(frameRow.GetString("Sheet") = "", "_", frameRow.GetString("Sheet"))
            output.TryAdd(pageNumber, pageRow.GetExtents3d("BlockExtents"))
        Next
        Return output
    End Function

    ''' <summary>
    ''' Gets a list of link codes and the objectids of linked bubbles.
    ''' </summary>
    ''' <returns>The <see cref="Dictionary"/> with link codes and objectids of linked bubbles.</returns>
    Private Function GetXlinkBubbles() As Dictionary(Of String, ObjectIdCollection)
        Dim output = New Dictionary(Of String, ObjectIdCollection)
        Dim textToChange = New Dictionary(Of ObjectId, String)
        Dim referenceIds = SelectHelper.GetAllReferencesInModelspace(_document, {"W_XLINKS"})
        DrawOrderHelper.BringToFront(_document, referenceIds)
        Dim linkData = ReferenceHelper.GetReferenceData(_database, referenceIds)
        For Each linkRow In linkData.Select
            Dim linkCode = linkRow.GetString("LINKCODE")
            If linkCode = "" Then
                linkCode = Randomizer.GetString(16)
                textToChange.TryAdd(linkRow.GetAttributeId("LINKCODE"), linkCode)
            End If
            Select Case output.ContainsKey(linkCode)
                Case True : output(linkCode).Add(linkRow.GetValue("ObjectID"))
                Case Else : output.Add(linkCode, New ObjectIdCollection({linkRow.GetValue("ObjectID")}))
            End Select
        Next
        TextHelper.ChangeTextStrings(_document, textToChange)
        Return output
    End Function

    ''' <summary>
    ''' Gets a collection of positions and bubble (blockreference) records.
    ''' </summary>
    ''' <param name="objectIds">List of objectids of bubbles linked together.</param>
    ''' <returns>The collection of positions and bubble (blockreference) records.</returns>
    Private Function GetLinkCollection(objectIds As ObjectIdCollection) As Dictionary(Of String, DataRow)
        Dim output = New Dictionary(Of String, DataRow)
        Dim linkData = ReferenceHelper.GetReferenceData(_database, objectIds)
        For Each linkRow In linkData.Select
            Dim position = GetXlinkPosition(linkRow.GetValue("Position"))
            Do While output.ContainsKey(position)
                Select Case position.EndsWith("$")
                    Case True : position = position.AutoNumber
                    Case Else : position = "{0}_1$".Compose(position)
                End Select
            Loop
            output.Add(position, linkRow)
        Next
        Return output
    End Function

    ''' <summary>
    ''' Gets page and position string of the specified point.
    ''' </summary>
    ''' <param name="point">The link bubble position.</param>
    ''' <returns>The page and postition string (eg. '003.12').</returns>
    Private Function GetXlinkPosition(point As Point3d) As String
        Dim linkPosition = "Not On Page"
        For Each linkPage In _linkPages
            Dim pageExtents = linkPage.Value
            Select Case True
                Case point.X < pageExtents.MinPoint.X
                Case point.Y < pageExtents.MinPoint.Y
                Case point.X > pageExtents.MaxPoint.X
                Case point.Y > pageExtents.MaxPoint.Y
                Case Else
                    Dim positionOnPage = Int((point.X - pageExtents.MinPoint.X) / 32.5) + 1
                    linkPosition = "{0}.{1}".Compose(linkPage.Key, "00{0}".Compose(positionOnPage).RightString(2))
                    Exit For
            End Select
        Next
        Return linkPosition
    End Function

End Class
