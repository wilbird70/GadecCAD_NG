'Gadec Engineerings Software (c) 2022
Imports System.Windows.Forms
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.Geometry
Imports GadecCAD_NG.Extensions

''' <summary>
''' Provides a legacy methode of updating the status in the double signature stamp.
''' </summary>
Public Class FrameStampHelper

    'subs

    ''' <summary>
    ''' Legacy method of updating the status in the double signature stamp.
    ''' <para>A dictionary is used to pass the selection of frames in the following way:</para>
    ''' <para>- Key: The fullname of a document containing selected frames.</para>
    ''' <para>- Value: The num-codes of the selected frames, separated by semicolons.</para>
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="frameSelection">The dictionary containing the selection of frames.</param>
    ''' <param name="frameListData">The drawinglist database of the present project (folder)</param>
    Public Shared Sub UpdateStatus(document As Document, frameSelection As Dictionary(Of String, String), frameListData As Data.DataTable)
        frameListData.AssignPrimaryKey("Filename;Num") 'primarykey toewijzen
        Dim documents = DocumentsHelper.GetOpenDocuments()
        Dim currentFileName = document.Name
        Dim dialog = New RevisionDialog(Registerizer.UserSetting("SignStampDrawnIni"), Registerizer.UserSetting("SignStampCheckIni"))
        If Not dialog.DialogResult = DialogResult.OK Then Exit Sub

        Dim revisionData = dialog.GetRevisionData.ToIniDictionary
        Registerizer.UserSetting("SignStampDrawnIni", revisionData("Drawn"))
        Registerizer.UserSetting("SignStampCheckIni", revisionData("Check"))
        If Not IsDate(revisionData("Date")) Then Exit Sub

        revisionData.TryAdd("Date1", revisionData("Date"))
        revisionData.TryAdd("Date2", revisionData("Date"))
        Try
            If frameSelection.Count > 5 Then Progressbar = New ProgressShow("ProcessingDocuments".Translate, frameSelection.Count)
            Dim filesFailedToSave = New List(Of String)
            Dim files = frameSelection.Keys.ToList
            For Each file In files
                If Progressbar?.CancelPressed Then Exit For

                Dim fileName = IO.Path.GetFileName(file)
                Progressbar?.PerformStep("Processing".Translate(fileName))
                Dim lock As DocumentLock = Nothing
                Dim db As Database = Nothing
                Try
                    Dim bitmap As Drawing.Bitmap = Nothing
                    Dim textToChange = New Dictionary(Of ObjectId, String)
                    Select Case documents.ContainsKey(file)
                        Case True
                            DocumentsHelper.Open(file)
                            lock = documents(file).LockDocument()
                            db = documents(file).Database
                        Case Else
                            db = New Database(False, True)
                            db.ReadDwgFile(file, FileOpenMode.OpenForReadAndWriteNoShare, False, "")
                            db.CloseInput(True)
                            bitmap = db.ThumbnailBitmap
                    End Select
                    Dim frameIdCollections = XRecordObjectIdsHelper.Load(db, "{Company}".Compose, "FrameWorkIDs")
                    If frameIdCollections.Count = 0 Then Continue For

                    Dim stampIdCollections = XRecordObjectIdsHelper.Load(db, "{Company}".Compose, "StampIDs")
                    Dim frameData = FrameHelper.BuildFrameData(db, frameIdCollections)
                    frameData.AssignPrimaryKey("Num")
                    If documents.ContainsKey(file) Then lock = documents(file).LockDocument()
                    Using import = New DefinitionsImporter("{Resources}\{0}".Compose("DC_Common_Frames.dwg"))
                        Dim definitionId = import.ImportDefinition(db, "JCI--_04_--1_DoubleSignStamp2020")
                        If definitionId.IsNull Then Continue For

                        For Each number In frameSelection(file).Cut
                            Dim frameRow = frameData.Rows.Find(number)
                            If IsNothing(frameRow) Then Continue For
                            If stampIdCollections.ContainsKey(number) AndAlso stampIdCollections(number)(0).IsErased Then stampIdCollections.Remove(number)
                            Dim spaceId = frameRow.GetObjectId("SpaceID")
                            Dim stampId = ObjectId.Null
                            Select Case stampIdCollections.ContainsKey(number)
                                Case True
                                    stampId = stampIdCollections(number)(0)
                                    ReferenceHelper.ChangeRotation(db, New ObjectIdCollection({stampId}), 3)
                                Case Else
                                    Dim extents = frameRow.GetExtents3d("BlockExtents")
                                    Dim scale = frameRow.GetDouble("ScaleFactor")
                                    Dim position = New Point3d(extents.MaxPoint.X - scale * 188, extents.MinPoint.Y + scale * 72, extents.MinPoint.Z)
                                    stampId = ReferenceHelper.InsertReference(db, spaceId, db.Clayer, definitionId, position, scale)
                                    If stampId.IsNull Then Continue For

                                    ReferenceHelper.ChangeScale(db, New ObjectIdCollection({stampId}), scale)
                                    ReferenceHelper.ChangeRotation(db, New ObjectIdCollection({stampId}), 3)
                                    stampIdCollections.TryAdd(number, New ObjectIdCollection({stampId}))
                            End Select
                            Select Case revisionData("Descr").Trim = ""
                                Case True
                                    ReferenceVisibilityHelper.SetProperty(db, stampId, "Invisible")
                                    revisionData("Drawn") = ""
                                    revisionData("Date1") = ""
                                    revisionData("Check") = ""
                                    revisionData("Date2") = ""
                                Case Else
                                    ReferenceVisibilityHelper.SetProperty(db, stampId, "Visible")
                                    DrawOrderHelper.BringToFront(db, spaceId, stampId)
                                    If revisionData("Drawn").Trim = "" Then revisionData("Date1") = ""
                                    If revisionData("Check").Trim = "" Then revisionData("Date2") = ""
                            End Select
                            Dim referenceData = ReferenceHelper.GetReferenceData(db, stampId)
                            If NotNothing(referenceData) Then
                                textToChange.TryAdd(referenceData.GetAttributeId("NAME"), "NAME".Translate)
                                textToChange.TryAdd(referenceData.GetAttributeId("DATE"), "DATE...".Translate)
                                textToChange.TryAdd(referenceData.GetAttributeId("PARAPH"), "PARAPH".Translate)
                                textToChange.TryAdd(referenceData.GetAttributeId("NAME1"), revisionData("Drawn"))
                                textToChange.TryAdd(referenceData.GetAttributeId("DATE1"), revisionData("Date1"))
                                textToChange.TryAdd(referenceData.GetAttributeId("NAME2"), revisionData("Check"))
                                textToChange.TryAdd(referenceData.GetAttributeId("DATE2"), revisionData("Date2"))
                                textToChange.TryAdd(referenceData.GetAttributeId("REVTEXTS"), revisionData("Descr"))
                            End If
                        Next
                    End Using

                    Dim dontEraseTheseIds = New ObjectIdCollection
                    'gebruikte 'tolist' is om de key-lijst los van de dictionary te maken
                    'dit om de collectie in de for-each loop te kunnen bewerken
                    Dim numbers = stampIdCollections.Keys.ToList '<-- tolist
                    For Each number In numbers
                        Select Case frameIdCollections.ContainsKey(number)
                            Case True : dontEraseTheseIds.Add(stampIdCollections(number)(0))
                            Case Else : stampIdCollections.Remove(number)
                        End Select
                    Next
                    DeleteOldStamps(db, dontEraseTheseIds)

                    XRecordObjectIdsHelper.Update(db, "{Company}".Compose, "StampIDs", stampIdCollections)
                    TextHelper.ChangeTextStrings(db, textToChange)
                    Select Case True
                        Case Not documents.ContainsKey(file)
                            Try
                                db.ThumbnailBitmap = bitmap
                                db.SaveAs(file, DwgVersion.Current)
                                Dim timeStamp = IO.File.GetLastWriteTime(file).ToTimeStamp
                                Dim rows = frameListData.Select("Filename='{0}'".Compose(fileName.Replace("'", "''")))
                                rows.ToList.ForEach(Sub(row) row("Filedate") = timeStamp)
                            Catch ex As Autodesk.AutoCAD.Runtime.Exception
                                filesFailedToSave.Add(fileName)
                            Catch ex As System.Exception
                                ex.Rethrow
                            End Try
                        Case Not file = document.Name : documents(file).EditorNeedsRegen(True)
                    End Select

                Catch ex As System.Exception
                    ex.Rethrow
                Finally
                    lock?.Dispose()
                    db?.Dispose()
                End Try
            Next
            If document.IsNamedDrawing Then frameListData.DataSet.WriteXml("{0}\Drawinglist.xml".Compose(IO.Path.GetDirectoryName(currentFileName)))
            If filesFailedToSave.Count > 0 Then MsgBox("NotSavedFiles".Translate(String.Join(vbLf, filesFailedToSave)), MsgBoxStyle.Exclamation)
        Catch ex As System.Exception
            ex.AddData($"StatusData: {String.Join(", ", revisionData)}")
            ex.ReThrow
        Finally
            Progressbar?.Dispose()
            Progressbar = Nothing
        End Try
    End Sub

    'private subs

    ''' <summary>
    ''' Legacy method of deleting old signature stamps.
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="dontEraseTheseIds">An <see cref="ObjectIdCollection"/> containing objectids of objects that will not be deleted.</param>
    Private Shared Sub DeleteOldStamps(database As Database, dontEraseTheseIds As ObjectIdCollection)
        Using tr = database.TransactionManager.StartTransaction
            Dim bt = tr.GetBlockTable(database.BlockTableId)
            For Each definitionId In bt
                Dim definition = tr.GetBlockTableRecord(definitionId)
                Select Case True
                    Case definition.Name.StartsWith("JCI--_04_--1_SIGNSTAMP") : EntityHelper.Delete(database, definition.GetBlockReferenceIds(False, False))
                    Case definition.Name.StartsWith("TYCO--_04_--1_SIGNSTAMP") : EntityHelper.Delete(database, definition.GetBlockReferenceIds(False, False))
                    Case definition.Name.StartsWith("TYCO--_04_--1_PARAAF") : EntityHelper.Delete(database, definition.GetBlockReferenceIds(False, False))
                    Case definition.Name.StartsWith("JCI--_04_--1_DoubleSign")
                        Dim referenceIds = definition.GetBlockReferenceIds(False, False).ToList
                        For Each anonymousBlockId In definition.GetAnonymousBlockIds.ToArray
                            definition = tr.GetBlockTableRecord(anonymousBlockId)
                            referenceIds.AddRange(definition.GetBlockReferenceIds(False, False).ToArray)
                        Next
                        'gebruikte 'toarray' is om de key-lijst los van de dictionary te maken
                        'dit om de collectie in de for-each loop te kunnen bewerken
                        For Each referenceId In referenceIds.ToArray
                            If dontEraseTheseIds.Contains(referenceId) Then referenceIds.Remove(referenceId)
                        Next
                        If referenceIds.Count > 0 Then EntityHelper.Delete(database, New ObjectIdCollection(referenceIds.ToArray))
                End Select
            Next
            tr.Commit()
        End Using
    End Sub

End Class
