'Gadec Engineerings Software (c) 2022
Imports System.Data
Imports System.Windows.Forms
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.EditorInput
Imports Autodesk.AutoCAD.Geometry
Imports GadecCAD_NG.Extensions

''' <summary>
''' Provides methods for inserting items (symbol, frame, or drawing area) at the specific scale into the current drawing.
''' <para>Methods includes:</para>
''' <para>- Setting the drawing scale.</para>
''' <para>- Select items from the designcenter.</para>
''' <para>- Project fire detectors by selecting the area.</para>
''' <para>- Project devices onto walls by selecting the wall and specifying the correct side.</para>
''' <para>- Checking the detection radius.</para>
''' </summary>
Public Class DesignMethods

    'subs

    ''' <summary>
    ''' A method that allows the user to change the drawing scale.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    Public Shared Function SetDrawingScale(document As Document) As Double
        Dim output = CDbl(SysVarHandler.GetVar("DIMSCALE"))
        Dim scales = DataSetHelper.LoadFromXml("{Support}\SetStandards.xml".Compose).GetTable("Scales").GetStringsFromColumn("Name")
        scales(0) = scales(0).Translate
        Dim dialog = New ListBoxDialog("SelectScale".Translate, scales, "1:{0}".Compose(output))
        If Not dialog.DialogResult = DialogResult.OK Then Return output

        If dialog.GetSelectedIndex > 0 Then
            output = scales(dialog.GetSelectedIndex).MidString(3).ToDouble
            SysVarHandler.SetVar(document, "DIMSCALE", output)
            Return output
        End If

        Dim dialog2 = New InputBoxDialog("Custom scale".Translate, output)
        If dialog2.InputText = "" Then Return output

        output = {dialog2.InputText.ToDouble, 1.0}.Max
        SysVarHandler.SetVar(document, "DIMSCALE", output)
        Return output
    End Function

    ''' <summary>
    ''' A method that allows the user to select and insert a symbol or (parts of) a drawing from a dialog.
    ''' <para>This method can be used from several commands (sessions) each remembering the last used page and choice.</para>
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="session">A string representing the session.</param>
    Public Shared Sub DesignCenter(document As Document, session As String)
        Dim db = document.Database
        Dim ed = document.Editor
        Dim scale = If(db.ModelSpaceIsCurrent, CDbl(SysVarHandler.GetVar("DIMSCALE")), 1.0)
        Dim sessionSettings = "{0};;".Compose(Registerizer.UserSetting(session)).Cut
        Dim dialog = New DesignCenter(sessionSettings, scale)
        If NotNothing(dialog.GetSession) Then Registerizer.UserSetting(session, Join(dialog.GetSession, ";"))
        If Not dialog.DialogResult = DialogResult.OK Or dialog.GetBlockName = "" Then Exit Sub

        SetDrawingScale(document, dialog.GetInsertScale)
        scale = If(dialog.GetAllowScale, dialog.GetInsertScale, 1.0)

        Using importer = New DefinitionsImporter("{Resources}\{0}".Compose(dialog.GetSourceFile))
            Dim definitionId = importer.ImportDefinition(document, dialog.GetBlockName)
            If definitionId.IsNull Then Exit Sub

            Using unitless = New UnitlessInsertion(db)
                ed.UpdateScreen()
                DocumentEvents.CommandCanceled = False
                Select Case dialog.GetAllowRotation
                    Case True : ed.Command("INSERT", dialog.GetBlockName, "SC", scale)
                    Case Else : ed.Command("INSERT", dialog.GetBlockName, "SC", scale, "R", 0)
                End Select
                If Not DocumentEvents.CommandCanceled Then ed.Command("EXPLODE", "L")
                DefinitionHelper.PurgeBlock(document, definitionId)
            End Using
        End Using
    End Sub

    ''' <summary>
    ''' A method that allows the user to insert an array of frames or symbols from a dialog.
    ''' <para>Usually used for inserting multiple frames.</para>
    ''' </summary>
    ''' <param name="document">The present document.</param>
    Public Shared Sub DesignArray(document As Document)
        Dim db = document.Database
        Dim ed = document.Editor
        Dim scale = If(db.ModelSpaceIsCurrent, CDbl(SysVarHandler.GetVar("DIMSCALE")), 1.0)
        Dim dialog = New DesignDialog("DA", scale)
        If Not dialog.DialogResult = DialogResult.OK OrElse dialog.GetSourceFile = "" OrElse dialog.GetBlockName = "" Then Exit Sub

        SetDrawingScale(document, dialog.GetInsertScale)
        scale = dialog.GetInsertScale
        Dim promptPointOptions = New PromptPointOptions("Sel First Corner:".Translate) With {.AllowNone = True}
        Dim selectPointResult = ed.GetPoint(promptPointOptions)
        If Not selectPointResult.Status = PromptStatus.OK Then Exit Sub

        Using importer = New DefinitionsImporter("{Resources}\{0}".Compose(dialog.GetSourceFile))
            Using sysVar = New SysVarHandler(document)
                Dim definitionId = importer.ImportDefinition(document, dialog.GetBlockName)
                If definitionId.IsNull Then Exit Sub

                Dim newFrameIds = New ObjectIdCollection
                sysVar.Set("OSMODE", 0)
                Dim arraySettings = dialog.GetArraySettings
                Dim numberOnX = arraySettings(0).ToInteger
                Dim numberOnY = arraySettings(1).ToInteger
                Dim distanceX = arraySettings(2).ToDouble
                Dim distanceY = arraySettings(3).ToDouble
                Dim frameInfoData = DataSetHelper.LoadFromXml("{Support}\SetFramesInfo.xml".Compose).GetTable("Frames", "Name") 'not needed, i think
                Dim frameNames = If(IsNothing(frameInfoData), "None".Cut, frameInfoData.GetStringsFromColumn("Name"))
                Dim basePoint = selectPointResult.Value
                Using tr = db.TransactionManager.StartTransaction
                    Dim entities = New List(Of Entity)
                    Dim referenceIds = New ObjectIdCollection
                    For y = 0 To numberOnY - 1
                        For x = 0 To numberOnX - 1
                            Dim position = basePoint + New Vector3d(x * distanceX * scale, -y * distanceY * scale, 0)
                            referenceIds.Add(ReferenceHelper.InsertReference(db, db.CurrentSpaceId, db.Clayer, definitionId, position, scale))
                            entities.AddRange(ReferenceHelper.CommandExplodeLast(document))
                        Next
                    Next
                    For Each entity In entities
                        If Not entity.GetDBObjectType = "BlockReference" Then Continue For

                        Dim reference = entity.CastAsBlockReference
                        If frameNames.Contains(reference.Name) Then newFrameIds.Add(reference.ObjectId)
                    Next
                    tr.Commit()
                End Using
                DefinitionHelper.PurgeBlock(document, definitionId)
                Dim frameData = FrameHelper.BuildFrameData(document, newFrameIds)
                Dim textToChange = New Dictionary(Of ObjectId, String)
                For i = 0 To newFrameIds.Count - 1
                    Dim frameRow = frameData.Rows.Find(newFrameIds(i))
                    textToChange.TryAdd(frameRow.GetAttributeId("Sheet"), "000{0}".Compose(i + 1).RightString(3))
                Next
                DocumentEvents.ObjectModifiedEnabled = False
                TextHelper.ChangeTextStrings(document, textToChange)
                DocumentEvents.ObjectModifiedEnabled = True
            End Using
        End Using
    End Sub

    ''' <summary>
    ''' A method that allows the user to insert symbols representing fire detectors.
    ''' <para>The projection will be based on the current standards as stated in the SetStandards.xml-file.</para>
    ''' </summary>
    ''' <param name="document">The present document.</param>
    Public Shared Sub DesignFireDetectors(document As Document)
        Dim db = document.Database
        Dim scale = If(db.ModelSpaceIsCurrent, CDbl(SysVarHandler.GetVar("DIMSCALE")), 1.0)
        Dim detectorData = DataSetHelper.LoadFromXml("{Support}\SetStandards.xml".Compose).GetTable("Detectors", "Name")
        Dim dialog = New DesignDialog("DD", scale)
        If Not dialog.DialogResult = DialogResult.OK Then Exit Sub

        SetDrawingScale(document, dialog.GetInsertScale)
        scale = dialog.GetInsertScale
        Dim definitionName = dialog.GetBlockName
        Dim selectedDevice = detectorData.Rows.Find(dialog.GetSelectedItem)
        Using importer = New DefinitionsImporter("{Resources}\{0}".Compose(dialog.GetSourceFile))
            Dim definitionId = importer.ImportDefinition(document, definitionName)
            If definitionId.IsNull Then Exit Sub

            FireDetectors(document, definitionId, selectedDevice, scale)
        End Using
    End Sub

    ''' <summary>
    ''' A method that allows the user to insert symbols on lines representing walls.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    Public Shared Sub DesignWallmountDevices(document As Document)
        Dim db = document.Database
        Dim scale = If(db.ModelSpaceIsCurrent, CDbl(SysVarHandler.GetVar("DIMSCALE")), 1.0)
        Dim wallmountData = DataSetHelper.LoadFromXml("{Support}\SetStandards.xml".Compose).GetTable("Wallmounts", "Name")
        Dim dialog = New DesignDialog("DW", scale)
        If Not dialog.DialogResult = DialogResult.OK Then Exit Sub

        SetDrawingScale(document, dialog.GetInsertScale)
        scale = dialog.GetInsertScale
        Dim rotation = dialog.GetRotation
        Dim definitionName = dialog.GetBlockName
        Using importer = New DefinitionsImporter("{Resources}\{0}".Compose(dialog.GetSourceFile))
            Dim definitionId = importer.ImportDefinition(document, definitionName)
            If definitionId.IsNull Then Exit Sub

            WallmountDevices(document, definitionId, scale, rotation)
        End Using
    End Sub

    ''' <summary>
    ''' Draws the temporary circles around the symbols so that the user can check their detection coverage.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    Public Shared Sub RadiusCheck(document As Document)
        Dim radiusData = DataSetHelper.LoadFromXml("{Support}\SetStandards.xml".Compose).GetTable("RadiusCheck")
        Dim choices = radiusData.GetStringsFromColumn(Translator.Selected)
        Dim previousChoice = {Registerizer.UserSetting("DetectorRadiusCheckSelected").ToInteger, choices.Count - 1}.Min
        Dim dialog = New ListBoxDialog("CheckRadius".Translate, choices, choices(previousChoice))
        If Not dialog.DialogResult = DialogResult.OK Then Exit Sub

        Registerizer.UserSetting("DetectorRadiusCheckSelected", dialog.GetSelectedIndex)
        Dim row = radiusData.Rows(dialog.GetSelectedIndex)
        RadiusCheck(document, row.GetString("Devices"), row.GetString("Name").ToInteger)
    End Sub

    'private subs

    ''' <summary>
    ''' Sets the AutoCAD system variable 'DIMSCALE' to the new scale.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="newScale">The new scale to use.</param>
    Private Shared Sub SetDrawingScale(document As Document, newScale As Double)
        If document.Database.ModelSpaceIsCurrent And Not newScale = SysVarHandler.GetVar("DIMSCALE") Then
            SysVarHandler.SetVar(document, "DIMSCALE", newScale)
        End If
    End Sub

    ''' <summary>
    ''' A method that allows the user to select an area for projecting the detectors.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="definitionId">The objectid of the symbol to use.</param>
    ''' <param name="selection">The users selection of the device.</param>
    ''' <param name="scale">The scale for the symbols.</param>
    Private Shared Sub FireDetectors(document As Document, definitionId As ObjectId, selection As DataRow, scale As Double)
        Dim db = document.Database
        Dim ed = document.Editor
        Dim promptPointOptions = New PromptPointOptions("Sel First Corner:".Translate) With {.AllowNone = True}
        Do
            Dim selectionResult = ed.GetPoint(promptPointOptions)
            If Not selectionResult.Status = PromptStatus.OK Then Exit Do

            Dim basePoint = selectionResult.Value
            Dim promptCornerOptions = New PromptCornerOptions("Sel Second Corner:".Translate, basePoint) With {.AllowNone = True}
            selectionResult = ed.GetCorner(promptCornerOptions)
            If Not selectionResult.Status = PromptStatus.OK Then Continue Do

            Dim cornerPoint = selectionResult.Value
            Dim lengthX = Math.Abs(basePoint.X - cornerPoint.X) / 1000
            Dim lengthY = Math.Abs(basePoint.Y - cornerPoint.Y) / 1000
            Dim area = lengthX * lengthY
            Dim maxArea = selection.GetString("A1").ToDouble
            Dim maxRadius = selection.GetString("D1").ToDouble
            If area > maxArea Then
                maxArea = selection.GetString("A").ToDouble
                maxRadius = selection.GetString("D").ToDouble
            End If
            If lengthX <= 3 Or lengthY <= 3 Then maxRadius = {selection.GetString("D2").ToDouble, maxRadius}.Max
            Dim minimumDetectors = {Int((area / maxArea) + 0.999), 1}.Max
            Dim solutions = New Dictionary(Of Integer, String)
            Dim numberOnX As List(Of Integer)
            Dim numberOnY As List(Of Integer)
            Do
                numberOnX = New List(Of Integer)
                numberOnY = New List(Of Integer)
                Dim count = 0, int1 = 1, int2 = 1
                Do
                    Select Case int1 > 0
                        Case True : numberOnX.Add(int2) : numberOnY.Add(Int((minimumDetectors / int2) + 0.999))
                        Case Else : numberOnY.Add(int2) : numberOnX.Add(Int((minimumDetectors / int2) + 0.999))
                    End Select
                    Select Case numberOnX(count) < numberOnY(count)
                        Case True : int2 += int1
                        Case Else : int2 = numberOnY(count) - 1 : int1 = -1
                    End Select
                    If int2 < 1 Then Exit Do
                    count += 1
                Loop
                For solutionId = 0 To count
                    Dim mathRadius = Math.Sqrt(((lengthX / (2 * numberOnX(solutionId))) ^ 2) + ((lengthY / (2 * numberOnY(solutionId))) ^ 2))
                    If mathRadius < maxRadius Then
                        solutions.Add(solutionId, "ProjectSolution".Translate(numberOnX(solutionId) * numberOnY(solutionId), numberOnY(solutionId), numberOnX(solutionId), mathRadius.ToFormatedString))
                    End If
                Next
                If solutions.Count > 0 Then Exit Do
                minimumDetectors += 1
            Loop
            Dim dialog = New ListBoxDialog("Select".Translate, solutions.Values.ToArray)
            If Not dialog.DialogResult = DialogResult.OK Then Continue Do

            Dim selectedSolutionId = solutions.Keys(dialog.GetSelectedIndex)
            Dim distanceX = lengthX / numberOnX(selectedSolutionId) * 1000
            Dim distanceY = lengthY / numberOnY(selectedSolutionId) * 1000
            Dim startX = {basePoint.X, cornerPoint.X}.Min + distanceX * 0.5
            Dim startY = {basePoint.Y, cornerPoint.Y}.Min + distanceY * 0.5
            Dim startPoint = New Point3d(startX, startY, 0)
            Using sysVar = New SysVarHandler(document)
                Autodesk.AutoCAD.Internal.Utils.SetUndoMark(True)
                sysVar.Set("OSMODE", 0)
                Dim referenceIds = New ObjectIdCollection
                For i = 0 To numberOnX(selectedSolutionId) - 1
                    For j = 0 To numberOnY(selectedSolutionId) - 1
                        Dim position = startPoint + New Vector3d(distanceX * i, distanceY * j, 0)
                        referenceIds.Add(ReferenceHelper.InsertReference(db, db.CurrentSpaceId, db.Clayer, definitionId, position, scale))
                        ReferenceHelper.CommandExplodeLast(document)
                    Next
                Next
                Autodesk.AutoCAD.Internal.Utils.SetUndoMark(False)
            End Using
        Loop
        DefinitionHelper.PurgeBlock(document, definitionId)
    End Sub

    ''' <summary>
    ''' A method that allows the user to select a point and angle on the wall for the detectors.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="definitionId">The objectid of the symbol to use.</param>
    ''' <param name="scale">The scale for the symbols.</param>
    ''' <param name="correctionRotation">Some symbols need to be rotated initially.</param>
    Private Shared Sub WallmountDevices(document As Document, definitionId As ObjectId, scale As Double, correctionRotation As Double)
        Dim db = document.Database
        Dim ed = document.Editor
        Dim angle = 0.0
        Dim promptPointOptions = New PromptPointOptions("Sel Wall Point:".Translate) With {.AllowNone = True}
        Do
            Using sysVar = New SysVarHandler(document)
                sysVar.Set("OSMODE", 512)
                Dim selectPointResult = ed.GetPoint(promptPointOptions)
                sysVar.Reset()
                If Not selectPointResult.Status = PromptStatus.OK Then Exit Do

                Dim pickedPoint = selectPointResult.Value
                Dim promptNestedEntityOptions = New PromptNestedEntityOptions("NonInteractivePickPoint") With {
                    .NonInteractivePickPoint = pickedPoint,
                    .UseNonInteractivePickPoint = True
                }
                Dim nestedEntityResult = ed.GetNestedEntity(promptNestedEntityOptions)
                If Not nestedEntityResult.Status = PromptStatus.OK Then Continue Do

                Using document.LockDocument
                    Using tr = db.TransactionManager.StartTransaction
                        Dim entity = tr.GetEntity(nestedEntityResult.ObjectId)
                        Select Case entity.GetDBObjectType
                            Case "Line" : angle = entity.CastAsLine.Angle
                            Case "Polyline"
                                Dim polyline = entity.CastAsPolyline
                                Dim segmentPoint3d = polyline.GetClosestPointTo(nestedEntityResult.PickedPoint, False)
                                Dim segmentPoint2d = segmentPoint3d.Add(polyline.StartPoint.GetAsVector()).Convert2d(polyline.GetPlane)
                                Dim vertex = 0
                                For i = 0 To polyline.NumberOfVertices - 1
                                    If polyline.OnSegmentAt(i, segmentPoint2d, 0.0) Then vertex = i : Exit For
                                Next
                                Dim lineSegment = polyline.GetLineSegmentAt(vertex)
                                angle = lineSegment.EndPoint.GetVectorTo(lineSegment.StartPoint).AngleOnPlane(polyline.GetPlane)
                            Case Else : Beep()
                        End Select
                        Dim promptAngleOptions = New PromptAngleOptions("Sel Wall Side:".Translate) With {.BasePoint = pickedPoint, .UseBasePoint = True, .AllowNone = True}
                        sysVar.Set("ORTHOMODE", 1)
                        sysVar.Set("SNAPANG", angle)
                        Dim selectAngleResult = ed.GetAngle(promptAngleOptions)
                        sysVar.Reset()
                        If Not selectAngleResult.Status = PromptStatus.OK Then Continue Do

                        Autodesk.AutoCAD.Internal.Utils.SetUndoMark(True)
                        sysVar.Set("OSMODE", 0)
                        Dim basePoint = pickedPoint.GetPolarPoint(selectAngleResult.Value, 3.5 * scale)
                        Dim rotation = selectAngleResult.Value + (((correctionRotation - 90) / 180) * Math.PI)
                        Dim referenceId = ReferenceHelper.InsertReference(db, db.CurrentSpaceId, db.Clayer, definitionId, basePoint, scale, rotation)
                        If referenceId = ObjectId.Null Then Continue Do

                        tr.Commit()
                    End Using
                    Dim entities = ReferenceHelper.CommandExplodeLast(document)
                    AttributesUpright(db, entities)
                    Autodesk.AutoCAD.Internal.Utils.SetUndoMark(False)
                End Using
            End Using
        Loop
        DefinitionHelper.PurgeBlock(document, definitionId)
    End Sub

    ''' <summary>
    ''' Rotates certain attributes of block references to put them upright.
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="entities">A list of entities.</param>
    Private Shared Sub AttributesUpright(database As Database, entities As Entity())
        Using tr = database.TransactionManager.StartTransaction
            For Each entity In entities
                If Not entity.GetDBObjectType = "BlockReference" Then Continue For

                Dim referenceRow = ReferenceHelper.GetReferenceData(database, entity.ObjectId)
                Dim reference = entity.CastAsBlockReference
                If Not reference.IsDynamicBlock Then Continue For

                Dim pointX = 0.0
                Dim pointY = 0.0
                Dim hasAngleProperty = False
                For Each referenceProperty In reference.DynamicBlockReferencePropertyCollection.ToArray
                    Select Case referenceProperty.PropertyName
                        Case "Position1 X" : pointX = reference.Position.X + referenceProperty.Value
                        Case "Position1 Y" : pointY = reference.Position.Y + referenceProperty.Value
                        Case "WngAng" : hasAngleProperty = True : referenceProperty.Value = Math.PI - reference.Rotation
                    End Select
                Next
                If hasAngleProperty Then Continue For

                Dim hasWinguardAttributes = referenceRow.HasValue("PNR") And referenceRow.HasValue("LS") And referenceRow.HasValue("ADR")
                If Not hasWinguardAttributes Then Continue For

                Dim point = New Point3d(pointX, pointY, reference.Position.Z)
                Dim attribute = tr.GetAttributeReference(referenceRow.GetAttributeId("PNR"), OpenMode.ForWrite)
                attribute.AlignmentPoint = New Point3d(point.X, point.Y + 2.5 * reference.ScaleFactors.X, point.Z)
                attribute.Rotation = 0
                attribute = tr.GetAttributeReference(referenceRow.GetAttributeId("LS"), OpenMode.ForWrite)
                attribute.AlignmentPoint = New Point3d(point.X - 1.25 * reference.ScaleFactors.X, point.Y, point.Z)
                attribute.Rotation = 0
                attribute = tr.GetAttributeReference(referenceRow.GetAttributeId("ADR"), OpenMode.ForWrite)
                attribute.AlignmentPoint = New Point3d(point.X - 0.9375 * reference.ScaleFactors.X, point.Y, point.Z)
                attribute.Rotation = 0
                If referenceRow.HasAttribute("DOT") Then
                    attribute = tr.GetAttributeReference(referenceRow.GetAttributeId("DOT"), OpenMode.ForWrite)
                    attribute.AlignmentPoint = New Point3d(point.X - 1.25 * reference.ScaleFactors.X, point.Y, point.Z)
                    attribute.Rotation = 0
                End If
            Next
            tr.Commit()
        End Using
    End Sub

    ''' <summary>
    ''' Draws the temporary circles around the symbols.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="devices">The semicolon-separated blocknames to use.</param>
    ''' <param name="radius">The radius to use.</param>
    Private Shared Sub RadiusCheck(document As Document, devices As String, radius As Double)
        Dim db = document.Database
        Dim ed = document.Editor
        Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView()
        Using transient = New TransientHandler
            Do
                Dim selectedReferences = SelectMethods.GetSelectionOfReferences(document, devices.Cut)
                If selectedReferences.Count = 0 Then Exit Do

                Dim referenceData = ReferenceHelper.GetReferenceData(db, selectedReferences)
                For Each row In referenceData.Select
                    transient.ShowCircle(row.GetValue("Position"), 500, True)
                    transient.ShowCircle(row.GetValue("Position"), radius, False)
                Next
                ed.UpdateScreen()
            Loop
        End Using
    End Sub

End Class
