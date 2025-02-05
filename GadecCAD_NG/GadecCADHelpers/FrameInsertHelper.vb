'Gadec Engineerings Software (c) 2022
Imports System.Data
Imports System.Windows.Forms
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.EditorInput
Imports Autodesk.AutoCAD.Geometry
Imports GadecCAD_NG.Extensions

''' <summary>
''' Provides methods for inserting, replacing and converting frames.
''' </summary>
Public Class FrameInsertHelper

    'subs

    ''' <summary>
    ''' Inserts frames to ModelSpace and filled-in based on the data in the framelist record.
    ''' <para>Data in the framelist record for insertion is used:</para>
    ''' <para>- Size: The particular size of the frame.</para>
    ''' <para>- Sheets: Number of sheets to insert.</para>
    ''' <para>Frames are not scaled or rotated. The distance between the frames is 50.</para>
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="frameListRow">The framelist record.</param>
    Public Shared Sub InsertFrames(document As Document, frameListRow As DataRow)
        Using document.LockDocument
            InsertFrames(document.Database, frameListRow)
        End Using
    End Sub

    ''' <summary>
    ''' Inserts frames to ModelSpace and filled-in based on the data in the framelist record.
    ''' <para>Data in the framelist record for insertion is used:</para>
    ''' <para>- Size: The particular size of the frame.</para>
    ''' <para>- Sheets: Number of sheets to insert.</para>
    ''' <para>Frames are not scaled or rotated. The distance between the frames is 50.</para>
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="frameListRow">The framelist record.</param>
    Public Shared Sub InsertFrames(database As Database, frameListRow As DataRow)
        Dim size = frameListRow.GetString("Size")
        Dim registryData = Registerizer.UserData("Frames-DataTable")
        registryData?.AssignPrimaryKey("Name")
        Dim registryRow = registryData?.Rows.Find(size)
        If IsNothing(registryRow) Then Exit Sub

        Dim blockName = registryRow.GetString("BlockName")
        Using import = New DefinitionsImporter("{Resources}\{0}".Compose(registryRow.GetString("SourceFile")))
            Dim definitionId = import.ImportDefinition(database, blockName)
            If definitionId.IsNull Then Exit Sub

            Dim sheet = 1
            Dim insertPointX = 0.0
            Dim numberOfSheets = frameListRow.GetString("Sheets").ToInteger
            For i = 0 To numberOfSheets - 1
                Dim newListRow = frameListRow.Table.NewRow
                For Each column In frameListRow.GetColumnNames
                    Select Case True
                        Case column = "Sheet" : newListRow(column) = sheet.ToString.PadLeft(3, "0")
                        Case Not frameListRow.GetString(column).StartsWith("[") : newListRow(column) = frameListRow.GetString(column)
                        Case Not frameListRow.GetString(column).EndsWith("]") : newListRow(column) = frameListRow.GetString(column)
                        Case Else
                            Dim texts = frameListRow.GetString(column).EraseStart(1).EraseEnd(1).Cut
                            newListRow(column) = If(i < texts.Count, texts(i), texts(texts.Count - 1))
                    End Select
                Next
                Dim position = New Point3d(insertPointX, 0, 0)
                Dim newFrameId = ReferenceHelper.InsertReference(database, SymbolUtilityServices.GetBlockModelSpaceId(database), database.Clayer, definitionId, position)
                insertPointX = ReferenceHelper.GetReferenceExtents(database, newFrameId).MaxPoint.X + 50
                Dim entities = ReferenceHelper.Explode(database, newFrameId)
                Dim referenceIds = entities.Where(Function(entity) entity.GetDBObjectType = "BlockReference").Select(Function(entity) entity.ObjectId)
                WriteAttributes(database, newListRow, New ObjectIdCollection(referenceIds.ToArray))
                sheet += 1
            Next
            DefinitionHelper.PurgeBlock(database, definitionId)
        End Using
    End Sub

    ''' <summary>
    ''' Replaces all existing frames in the document with the defined default frames.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="withDialog">If true, a dialog box will be displayed to change the default settings.</param>
    Public Shared Sub ReplaceFrames(document As Document, Optional withDialog As Boolean = False)
        If withDialog Then
            Dim dialog = New DesignDialog("DF", 1.0)
            If Not dialog.DialogResult = DialogResult.OK Then Exit Sub
        End If
        ReplaceFrames(document, document.FrameData)
        Dim frameIdCollections = FrameHelper.GetFrameIdCollections(document.FrameData(True))
        XRecordObjectIdsHelper.Update(document, "{Company}".Compose, "FrameWorkIDs", frameIdCollections)
    End Sub

    ''' <summary>
    ''' Converts a selected (rectangular) polyline to a frame so that it can be treated as a frame.
    ''' <para>If no polyline is selected, it prompts to specify a rectangle to convert.</para>
    ''' </summary>
    ''' <param name="document">The present document.</param>
    Public Shared Sub ConvertPolylineToFrame(document As Document)
        Dim db = document.Database
        Dim ed = document.Editor
        Dim LayerHandler = New LayerHandler(db)
        Dim layerId = LayerHandler.Create("X--L$02A__Frame", 3)
        Dim promptEntityOptions = New PromptEntityOptions("Sel Polyline:".Translate)
        Do
            Dim selectionResult = ed.GetEntity(promptEntityOptions)
            Dim entity As Entity

            Select Case selectionResult.Status = PromptStatus.OK
                Case True
                    Using tr = db.TransactionManager.StartTransaction()
                        entity = tr.GetEntity(selectionResult.ObjectId)
                        tr.Commit()
                    End Using
                Case Else
                    Dim promptPointOptions = New PromptPointOptions("Sel First Corner:".Translate) With {.AllowNone = True}
                    Dim selectPointResult = ed.GetPoint(promptPointOptions)
                    If Not selectPointResult.Status = PromptStatus.OK Then Exit Sub

                    Dim basePoint = selectPointResult.Value
                    Dim promptCornerOptions = New PromptCornerOptions("Sel Second Corner:".Translate, basePoint) With {.AllowNone = True}
                    selectPointResult = ed.GetCorner(promptCornerOptions)
                    If Not selectPointResult.Status = PromptStatus.OK Then Exit Sub

                    Dim cornerPoint = selectPointResult.Value
                    Dim vertices = New Point2dCollection From {
                        New Point2d(basePoint.X, basePoint.Y),
                        New Point2d(basePoint.X, cornerPoint.Y),
                        New Point2d(cornerPoint.X, cornerPoint.Y),
                        New Point2d(cornerPoint.X, basePoint.Y)
                    }
                    entity = PolylineHelper.Create(vertices, True)
                    entity.LayerId = layerId
            End Select
            If Not entity.GetDBObjectType = "Polyline" Then Autodesk.AutoCAD.Internal.Utils.PostCommandPrompt() : Continue Do

            Dim polyLine = entity.CastAsPolyline
            Dim extents = polyLine.GeometricExtents
            Dim possibleFrames = GetPossibleFrames(extents).ToArray
            If possibleFrames.Count = 0 Then MsgBox("No matching frame found".Translate) : Continue Do

            Dim items = possibleFrames.Select(Function(frame) "Frame-Scale".Translate(frame.Value.GetString("Name"), frame.Value.GetString("Scale")))
            Dim dialog = New ListBoxDialog("SelectScale".Translate, items.ToArray)
            If Not dialog.DialogResult = DialogResult.OK Then Exit Do

            Dim index = dialog.GetSelectedIndex
            Dim scale = possibleFrames(index).Key
            Dim frameSizeRow = possibleFrames(index).Value
            'voorbereiding
            If polyLine.IsNewObject Then EntityHelper.Add(document, polyLine)
            Dim width = frameSizeRow.GetDouble("Width")
            Dim height = frameSizeRow.GetDouble("Height")
            Dim offsetX = (width - (extents.MaxPoint.X - extents.MinPoint.X) / scale) / 2
            Dim offsetY = (height - (extents.MaxPoint.Y - extents.MinPoint.Y) / scale) / 2
            'start verzameling tekenobjecten
            Dim newEntities = New List(Of Entity)
            'vier puntjes 
            Dim insertionPoint = New Point3d(0, 0, 0)
            Dim frameExtents = New Extents3d(insertionPoint, New Point3d(width, height, 0))
            For Each surroundingPoint In frameExtents.GetSurroundingPoints
                Dim newLine = New Line(surroundingPoint, surroundingPoint)
                newEntities.Add(New Line(surroundingPoint, surroundingPoint))
            Next
            'de attributen
            Dim attributeDefinitions = InsertAttributeDefinitions(document, insertionPoint, frameSizeRow.GetString("Name"), frameSizeRow.GetString("Scale"))
            newEntities.AddRange(attributeDefinitions)
            'entities tot nu toe op onzichbare laag plaatsen
            Dim invisibleLayerID = LayerHandler.Create("-Gadec-off", 10, False)
            newEntities.ForEach(Sub(ent) ent.SetLayerId(invisibleLayerID, True))
            'de polyline
            Dim polylineExtents = New Extents2d(New Point2d(0 + offsetX, 0 + offsetY), New Point2d(width - offsetX, height - offsetY))
            Dim newPolyLine = PolylineHelper.Create(polylineExtents.GetSurroundingPoints, True)
            newPolyLine.SetLayerId(polyLine.LayerId, False)
            newPolyLine.ColorIndex = 256
            newEntities.Add(newPolyLine)
            'de blokdefinitie maken
            Dim blockName = "FRAME_X{0}$_{1}".Compose(frameSizeRow.GetString("Name"), Randomizer.GetString(6))
            Dim newDefinitionId = DefinitionHelper.CreateDefinition(db, newEntities.ToArray, blockName)
            'en invoegen...
            If Not newDefinitionId.IsNull Then
                Dim position = New Point3d(extents.MinPoint.X - (offsetX * scale), extents.MinPoint.Y - (offsetY * scale), extents.MinPoint.Z)
                ReferenceHelper.InsertReference(document, db.CurrentSpaceId, layerId, newDefinitionId, position, scale)
                EntityHelper.Delete(document, polyLine.ObjectId)
            End If
            Exit Do
        Loop
    End Sub

    'private subs

    ''' <summary>
    ''' Replaces all existing frames in the document with the defined default frames.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="frameData">The frame database.</param>
    Private Shared Sub ReplaceFrames(document As Document, frameData As Data.DataTable)
        Dim db = document.Database
        Dim ed = document.Editor
        Dim registryData = Registerizer.UserData("Frames-DataTable")
        If IsNothing(registryData) Then Exit Sub

        registryData.AssignPrimaryKey("Name")
        Dim layoutManager = Autodesk.AutoCAD.DatabaseServices.LayoutManager.Current
        Dim currentLayoutId = layoutManager.GetLayoutId(layoutManager.CurrentLayout)
        Dim definitionIds = New ObjectIdCollection
        Dim frameInfoSet = DataSetHelper.LoadFromXml("{Support}\SetFramesInfo.xml".Compose)
        If Not frameInfoSet.Tables.Contains("Frames") Or Not frameInfoSet.Tables.Contains("Headers") Then Exit Sub

        Dim frameNames = frameInfoSet.GetTable("Frames").GetStringsFromColumn("Name")
        Dim headerNames = frameInfoSet.GetTable("Headers").GetStringsFromColumn("Name")
        Dim newFrameIds = New Dictionary(Of ObjectId, ObjectIdCollection)
        Dim dataMapping = New Dictionary(Of ObjectId, DataRow)
        Using sysVar = New SysVarHandler(document)
            sysVar.Set("OSMODE", 0)
            Using document.LockDocument
                Using tr = db.TransactionManager.StartTransaction
                    For Each row In frameData.Select
                        Dim registryRow = registryData.Rows.Find(row("FrameSize"))
                        If IsNothing(registryRow) Then Continue For

                        Dim bt = tr.GetBlockTable(db.BlockTableId)
                        Dim blockName = registryRow.GetString("BlockName")
                        Dim definitionId = If(bt.Has(blockName), bt(blockName), New ObjectId)
                        If definitionId.IsNull Then
                            Using importer = New DefinitionsImporter("{Resources}\{0}".Compose(registryRow.GetString("SourceFile")))
                                definitionId = importer.ImportDefinition(document, blockName)
                            End Using
                        End If
                        If definitionId.IsNull Then Continue For

                        If Not definitionIds.Contains(definitionId) Then definitionIds.Add(definitionId)
                        LayoutHelper.SelectById(document, row.GetObjectId("LayoutID"))
                        EntityHelper.Delete(document, row.GetObjectId("ObjectID"))
                        EntityHelper.Delete(document, row.GetObjectIdCollection("ObjectIDs"))
                        Dim basePoint = row.GetExtents3d("BlockExtents").MinPoint
                        Dim scale = row.GetDouble("ScaleFactor")
                        Dim referenceId = ReferenceHelper.InsertReference(db, db.CurrentSpaceId, db.Clayer, definitionId, basePoint, scale)

                        Dim entities = ReferenceHelper.ExplodeToOwnerSpace(db, referenceId)
                        Dim frameId = ObjectId.Null
                        Dim frameIds = New ObjectIdCollection
                        For Each entity In entities
                            If Not entity.GetDBObjectType = "BlockReference" Then Continue For

                            Dim reference = entity.CastAsBlockReference
                            Select Case True
                                Case frameNames.Contains(reference.Name) : frameId = reference.ObjectId
                                Case headerNames.Contains(reference.Name) : frameIds.Add(reference.ObjectId)
                            End Select
                        Next
                        If Not frameId = ObjectId.Null Then
                            frameIds.Insert(0, frameId)
                            newFrameIds.TryAdd(frameId, frameIds)
                            dataMapping.TryAdd(frameId, row)
                        End If
                    Next
                    tr.Commit()
                End Using
            End Using
        End Using
        DefinitionHelper.PurgeBlocks(document, definitionIds)
        LayoutHelper.SelectById(document, currentLayoutId)
        Dim textToChange = New Dictionary(Of ObjectId, String)
        For Each pair In dataMapping
            WriteAttributes(document, pair.Value, newFrameIds(pair.Key))
        Next
    End Sub

    ''' <summary>
    ''' Writes the data from the framelist record to the header attributes of the frames.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="frameListRow">The framelist record.</param>
    ''' <param name="frameIds">A <see cref="ObjectIdCollection"/> containing the objectids of the frame and its headers.</param>
    Private Shared Sub WriteAttributes(document As Document, frameListRow As DataRow, frameIds As ObjectIdCollection)
        Dim db = document.Database
        Using document.LockDocument
            WriteAttributes(document.Database, frameListRow, frameIds)
        End Using
    End Sub

    ''' <summary>
    ''' Writes the data from the framelist record to the header attributes of the frames.
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="frameListRow">The framelist record.</param>
    ''' <param name="frameIds">A <see cref="ObjectIdCollection"/> containing the objectids of the frame and its headers.</param>
    Private Shared Sub WriteAttributes(database As Database, frameListRow As DataRow, frameIds As ObjectIdCollection)
        If frameIds(0).IsNull Or Not frameIds(0).IsValid Then Exit Sub

        Dim frameIdCollections = New Dictionary(Of String, ObjectIdCollection) From {{Randomizer.GetString(6), frameIds}}
        XRecordObjectIdsHelper.Save(database, "{Company}".Compose, "FrameWorkIDs", frameIdCollections)
        Dim frameRows = FrameHelper.BuildFrameData(database, frameIdCollections).Rows.ToList
        If frameRows.Count = 0 Then Exit Sub

        Dim targetRow = frameRows.First
        Dim textToChange = New Dictionary(Of ObjectId, String)
        Dim revisionData = New Dictionary(Of Date, String)
        Dim attributeColumns = targetRow.GetAttributeColumns
        For Each column In attributeColumns
            If column.StartsWith("Rev#") Then Continue For

            Dim value = ""
            Select Case True
                Case frameListRow.HasValue(column)
                    value = frameListRow.GetString(column)
                    Select Case column
                        Case "Scale" : value = FrameHelper.GetScaleFactor(database, targetRow.GetValue("ObjectID"))
                        Case "Size" : value = targetRow.GetString("FrameSize")
                        Case "Drawn", "Design" : value = GetUserInitial(value)
                        Case "Date" : value = DateStringHelper.Convert(value)
                    End Select
                Case Else
                    Select Case column
                        Case "Scale" : value = FrameHelper.GetScaleFactor(database, targetRow.GetValue("ObjectID"))
                        Case "Size" : value = targetRow.GetString("FrameSize")
                        Case "Char" : value = "0"
                        Case "Descr" : value = "DESIGNED".Translate
                    End Select
            End Select
            textToChange.TryAdd(targetRow.GetAttributeId(column), value)
        Next
        'uitlezen van in te vullen revisiedata
        For Each column In frameListRow.GetAttributeColumns
            Dim number = column.InStrResult("Rev#", "_Date")
            If number = "" Then Continue For

            Dim revDate = DateStringHelper.Convert(frameListRow.GetString(column))
            If IsDate(revDate) Then revisionData.TryAdd(CDate(revDate), number)
        Next
        'bepalen van revisieplaatsen
        Dim revisionAttributeCount = 0
        Do While attributeColumns.Contains("Rev#{0}_Date".Compose(revisionAttributeCount + 1))
            revisionAttributeCount += 1
        Loop
        'als er meer revisiedata zijn dan revisieplaatsen
        Dim sortedDates = revisionData.Keys.ToSortedList
        Do While sortedDates.Count > revisionAttributeCount
            sortedDates.RemoveAt(0)
        Loop
        'overnemen van revisiegegevens
        For i = 0 To sortedDates.Count - 1
            Dim sourceTag = "Rev#{0}".Compose(revisionData(sortedDates(i)))
            Dim targetTag = "Rev#{0}".Compose(i + 1)
            textToChange.TryAdd(targetRow.GetAttributeId("{0}_Date".Compose(targetTag)), Format(sortedDates(i), "dd-MM-yyyy"))
            If targetRow.HasAttribute("{0}_Char".Compose(targetTag)) Then textToChange.TryAdd(targetRow.GetAttributeId("{0}_Char".Compose(targetTag)), frameListRow.GetString("{0}_Char".Compose(sourceTag)))
            If targetRow.HasAttribute("{0}_Drawn".Compose(targetTag)) Then textToChange.TryAdd(targetRow.GetAttributeId("{0}_Drawn".Compose(targetTag)), GetUserInitial(frameListRow.GetString("{0}_Drawn".Compose(sourceTag))))
            If targetRow.HasAttribute("{0}_Descr".Compose(targetTag)) Then textToChange.TryAdd(targetRow.GetAttributeId("{0}_Descr".Compose(targetTag)), frameListRow.GetString("{0}_Descr".Compose(sourceTag)))
            Select Case True
                Case Not targetRow.HasAttribute("{0}_KOPREV".Compose(targetTag))
                Case Not frameListRow.GetString("{0}_KOPREV".Compose(sourceTag)) = ""
                    textToChange.TryAdd(targetRow.GetAttributeId("{0}_KOPREV".Compose(targetTag)), frameListRow.GetString("{0}_KOPREV".Compose(sourceTag)))
                Case Else
                    Dim text = "{0} ({1})".Compose(frameListRow.GetString("{0}_Char".Compose(sourceTag)), GetUserInitial(frameListRow.GetString("{0}_Drawn".Compose(sourceTag))))
                    textToChange.TryAdd(targetRow.GetAttributeId("{0}_KOPREV".Compose(targetTag)), text)
            End Select
        Next
        DocumentEvents.ObjectModifiedEnabled = False
        TextHelper.ChangeTextStrings(database, textToChange)
        DocumentEvents.ObjectModifiedEnabled = True
    End Sub

    'private functions

    ''' <summary>
    ''' Gets the initials of an employee if his name is found in the 'SetUsers.xml'.
    ''' <para>The function is used to go back from fullnames to initials of employees.</para>
    ''' </summary>
    ''' <param name="employeeName">The name of the employee.</param>
    ''' <returns></returns>
    Private Shared Function GetUserInitial(employeeName As String) As String
        Static userData As Data.DataTable
        Dim output = employeeName
        If IsNothing(userData) Then userData = DataSetHelper.LoadFromXml("{Support}\SetUsers.xml".Compose).GetTable("Users", "Name")
        If NotNothing(userData) Then
            Dim allUsers = userData.GetUniqueStringsFromColumn("Name")
            For Each row In userData.Select
                If employeeName.Contains(row.GetString("Name")) Then
                    output = row.GetString("Initial")
                    Exit For
                End If
            Next
        End If
        Return output
    End Function

    ''' <summary>
    ''' Gets all the possible frames based on the extents.
    ''' </summary>
    ''' <param name="extents">The extents of the polyline.</param>
    ''' <returns>An dictionary containing frame-properties (value) per possible scale (key).</returns>
    Private Shared Function GetPossibleFrames(extents As Extents3d) As Dictionary(Of Double, DataRow)
        Dim frameSizeData = DataSetHelper.LoadFromXml("{Support}\SetStandards.xml".Compose).GetTable("Frames", "Name")
        frameSizeData.AssignPrimaryKey("Area")
        Dim possibleFrames = New Dictionary(Of Double, DataRow)
        Dim frameWidth = extents.MaxPoint.X - extents.MinPoint.X
        Dim frameHeight = extents.MaxPoint.Y - extents.MinPoint.Y
        Dim standardScales = {5000, 2000, 1000, 500, 200, 150, 100, 50, 20, 10, 5, 2, 1, 1 / 25.4}
        Dim possibleHeights = New Dictionary(Of Double, Integer)
        Dim heights = frameSizeData.GetUniqueStringsFromColumn("Height").Select(Function(h) CInt(Val(h))).ToSortedList
        For Each scale In standardScales
            Dim scaleRange = {scale * 0.7, scale * 1.002}
            For Each height In heights
                If frameHeight < height * scaleRange.Min Or frameHeight > height * scaleRange.Max Then Continue For

                possibleHeights.TryAdd(scale, height)
                Exit For
            Next
        Next
        For Each possibleHeight In possibleHeights.ToArray
            Dim scaleRange = {possibleHeight.Key * 0.7, possibleHeight.Key * 1.002}
            Dim widths = frameSizeData.Rows.ToArray.Where(Function(row) row.GetString("Height") = possibleHeight.Value).Select(Function(row) CInt(row.GetString("Width"))).ToSortedList
            For Each width In widths
                If frameWidth > width * scaleRange.Max Then Continue For

                Dim row = frameSizeData.Rows.Find(width * possibleHeight.Value)
                Select Case possibleHeight.Key < 1
                    Case True : row.SetString("Scale", "{0}:1".Compose(1 / possibleHeight.Key))
                    Case Else : row.SetString("Scale", "1:{0}".Compose(possibleHeight.Key))
                End Select
                possibleFrames.TryAdd(possibleHeight.Key, frameSizeData.Rows.Find(width * possibleHeight.Value))
                Exit For
            Next
        Next
        Return possibleFrames
    End Function

    ''' <summary>
    ''' Inserts attribute-definitions to add to a frame-definition.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="insertionPoint">Insertionpoint for the new attribute-definitions (all in one place).</param>
    ''' <param name="size">Default value for the 'size' attribute.</param>
    ''' <param name="scale">Default value for the 'scale' attribute.</param>
    ''' <returns>An array of attribute-definitions.</returns>
    Private Shared Function InsertAttributeDefinitions(document As Document, insertionPoint As Point3d, size As String, scale As String) As Entity()
        Dim entities = New List(Of Entity)
        Dim textStyleId = TextStyleHelper.GetTextStyleId(document.Database, "ISO_94")
        Dim newAttributeDefinition = Function(value As String, tag As String, prompt As String) New AttributeDefinition(insertionPoint, value, tag, prompt, textStyleId) With {.Height = 1.8}
        entities.Add(newAttributeDefinition("", "FOLNR", "Dossier"))
        entities.Add(newAttributeDefinition("", "GEBR", "Client 1"))
        entities.Add(newAttributeDefinition("", "ADR", "Client 2"))
        entities.Add(newAttributeDefinition("", "P_CODE", "Client 3"))
        entities.Add(newAttributeDefinition("", "PL", "Client 4"))
        entities.Add(newAttributeDefinition(document.GetFileNameWithoutExtension, "TEKNR", "Drawing"))
        entities.Add(newAttributeDefinition(document.AutoNumber, "BLD", "Sheet"))
        entities.Add(newAttributeDefinition("", "OMSCHR1", "Description 1"))
        entities.Add(newAttributeDefinition("", "OMSCHR2", "Description 2"))
        entities.Add(newAttributeDefinition("", "OMSCHR3", "Description 3"))
        entities.Add(newAttributeDefinition("", "OMSCHR4", "Description 4"))
        entities.Add(newAttributeDefinition("", "PROJNR", "Project"))
        entities.Add(newAttributeDefinition(scale, "SCALE", "Scale"))
        entities.Add(newAttributeDefinition(size, "FORM", "Size"))
        entities.Add(newAttributeDefinition("", "REVNR", "Revision"))
        entities.Add(newAttributeDefinition("-", "D0", "Date"))
        entities.Add(newAttributeDefinition("-", "OMSCHR_REV0", "Description"))
        entities.Add(newAttributeDefinition("", "RA", "Revision"))
        entities.Add(newAttributeDefinition("", "DA", "Revision date"))
        entities.Add(newAttributeDefinition("", "OMSCHR_REVA", "Revision description"))
        Return entities.ToArray
    End Function

End Class
