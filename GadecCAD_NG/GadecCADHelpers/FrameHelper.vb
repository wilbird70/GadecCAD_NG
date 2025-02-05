'Gadec Engineerings Software (c) 2022
Imports System.Data
Imports System.Windows.Forms
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports GadecCAD_NG.Extensions

''' <summary>
''' Provides methods for creating and handling frame databases.
''' </summary>
Public Class FrameHelper

    'functions

    ''' <summary>
    ''' Creates a drawing database for the frames specified by their objectids.
    ''' <para>Each frame is identified by the objectid of the blockreference representing that frame.</para>
    ''' <para>To find all data, the methode will search for headers within the extends of the frame.</para>
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="frameIds">A collection containing an <see cref="ObjectId"/> per frame.</param>
    ''' <returns>The drawing database.</returns>
    Public Shared Function BuildFrameData(document As Document, frameIds As ObjectIdCollection) As Data.DataTable
        Dim db = document.Database
        'first use existing numbers and then create new ones.
        Dim frameIdCollections = XRecordObjectIdsHelper.Load(db, "{Company}".Compose, "FrameWorkIDs")
        Dim currentFrameIds = New List(Of ObjectId)
        'the 'tolist' is used to separate the keylist from the dictionary.
        'this allows editing the collection in the for-each loop.
        Dim numbers = frameIdCollections.Keys.ToList '<-- tolist
        For Each number In numbers
            Dim frameIdByNumber = frameIdCollections(number).Item(0)
            Select Case True
                Case Not frameIds.Contains(frameIdByNumber) : frameIdCollections.Remove(number)
                Case currentFrameIds.Contains(frameIdByNumber) : frameIdCollections.Remove(number)
                Case Else : currentFrameIds.Add(frameIdByNumber)
            End Select
        Next
        For Each objectId In frameIds.ToArray
            If Not currentFrameIds.Contains(objectId) Then
                Dim number = Randomizer.GetString(6)
                frameIdCollections.TryAdd(number, New ObjectIdCollection({objectId}))
                document.ActiveFrame(number)
            End If
        Next
        Dim dataBuilder = New DataBuilder("FrameTable", "Dossier;Drawing;Sheet;FrameSize;LayoutName".Cut)
        dataBuilder.InsertColumn("ObjectID", GetType(ObjectId), 0)
        Using tr = db.TransactionManager.StartTransaction
            For Each pair In frameIdCollections
                Dim frameId = pair.Value.Item(0)
                If frameId.IsErased Then Continue For

                Dim frame = tr.GetBlockReference(frameId)
                If Not frame.Bounds.HasValue Or frame.OwnerId.IsErased Then Continue For

                Dim btr = tr.GetBlockTableRecord(frame.OwnerId)
                If Not btr.IsLayout Then Continue For

                Dim layout = tr.GetLayout(btr.LayoutId)
                Dim objectIds = FrameFindHelper.HeadersByFrame(document, layout.LayoutName, frame.GeometricExtents, frame.ScaleFactors.X)
                objectIds.Insert(0, frameId)
                dataBuilder.AppendValue("Filename", IO.Path.GetFileName(document.Name))
                dataBuilder.AppendValue("Filedate", IO.File.GetLastWriteTime(document.Name).ToTimeStamp)
                dataBuilder.AppendValue("Num", pair.Key)
                dataBuilder.AppendValue("ObjectID", frameId)
                dataBuilder.AppendValue("ObjectIDs", objectIds)
                dataBuilder.AppendValue("SpaceID", frame.OwnerId)
                dataBuilder.AppendValue("LayoutID", btr.LayoutId)
                dataBuilder.AppendValue("LayoutName", layout.LayoutName)
                dataBuilder.AppendValue("BlockName", frame.Name)
                dataBuilder.AppendValue("Position", frame.Position)
                dataBuilder.AppendValue("ScaleFactor", frame.ScaleFactors.X)
                dataBuilder.AppendValue("Rotation", frame.Rotation)
                dataBuilder.AppendValue("BlockExtents", frame.GeometricExtents)
                Dim frameHasFamily = AppendHeaderData(tr, dataBuilder, objectIds)
                Select Case frameHasFamily
                    Case True : dataBuilder.AddNewlyCreatedRow()
                    Case Else : dataBuilder.RejectRow()
                End Select
            Next
            tr.Commit()
        End Using
        Return dataBuilder.GetDataTable("ObjectID", "Dossier;Drawing;Sheet")
    End Function

    ''' <summary>
    ''' Creates a drawing database for the frames specified by the objectidcollections.
    ''' <para>Each frame is identified by an <see cref="ObjectIdCollection"/> containing the objectids of the blockreferences representing the frame and its headers.</para>
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="frameIdCollections">A dictionary containing an <see cref="ObjectIdCollection"/> per frame.</param>
    ''' <returns>The drawing database.</returns>
    Public Shared Function BuildFrameData(database As Database, frameIdCollections As Dictionary(Of String, ObjectIdCollection)) As Data.DataTable
        Dim dataBuilder = New DataBuilder("FrameTable", "Dossier;Drawing;Sheet;FrameSize;LayoutName".Cut)
        dataBuilder.InsertColumn("ObjectID", GetType(ObjectId), 0)
        Using tr = database.TransactionManager.StartTransaction
            For Each pair In frameIdCollections
                Dim frameIds = pair.Value
                Dim frameId = frameIds.Item(0)
                If frameId.IsErased Then Continue For

                Dim frame = tr.GetBlockReference(frameId)
                If Not frame.Bounds.HasValue Or frame.OwnerId.IsErased Then Continue For

                Dim btr = tr.GetBlockTableRecord(frame.OwnerId)
                If Not btr.IsLayout Then Continue For

                Dim layout = tr.GetLayout(btr.LayoutId)
                frameIds.Insert(0, frameId)
                dataBuilder.AppendValue("Num", pair.Key)
                dataBuilder.AppendValue("ObjectID", frameId)
                dataBuilder.AppendValue("ObjectIDs", frameIds)
                dataBuilder.AppendValue("SpaceID", frame.OwnerId)
                dataBuilder.AppendValue("LayoutID", btr.LayoutId)
                dataBuilder.AppendValue("LayoutName", layout.LayoutName)
                dataBuilder.AppendValue("BlockName", frame.Name)
                dataBuilder.AppendValue("Position", frame.Position)
                dataBuilder.AppendValue("ScaleFactor", frame.ScaleFactors.X)
                dataBuilder.AppendValue("Rotation", frame.Rotation)
                dataBuilder.AppendValue("BlockExtents", frame.GeometricExtents)
                Dim frameHasFamily = AppendHeaderData(tr, dataBuilder, frameIds)
                Select Case frameHasFamily
                    Case True : dataBuilder.AddNewlyCreatedRow()
                    Case Else : dataBuilder.RejectRow()
                End Select
            Next
            tr.Commit()
        End Using
        Return dataBuilder.GetDataTable("ObjectID", "Dossier;Drawing;Sheet")
    End Function

    ''' <summary>
    ''' Creates a drawingrecord for the header (with the absence of a frame) found in the current layout of the document.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <returns>The drawingrecord.</returns>
    Public Shared Function BuildFramelessRow(document As Document) As DataRow
        Dim db = document.Database
        Dim headerIds = FrameFindHelper.FramelessHeaders(document, LayoutHelper.GetCurrentName)
        If headerIds.Count = 0 Then MessageBoxInfo("NoHeaderFound".Translate) : Return Nothing

        headerIds.Insert(0, ObjectId.Null)
        Dim dataBuilder = New DataBuilder("Framelist", "Dossier;Drawing;Sheet;Size;LayoutName".Cut)
        dataBuilder.InsertColumn("ObjectID", GetType(ObjectId), 0)
        Using tr = db.TransactionManager.StartTransaction
            Dim header = tr.GetBlockReference(headerIds(1))
            dataBuilder.AppendValue("LayoutID", LayoutHelper.GetCurrentId)
            dataBuilder.AppendValue("LayoutName", LayoutHelper.GetCurrentName)
            dataBuilder.AppendValue("ObjectID", headerIds(1))
            dataBuilder.AppendValue("ScaleFactor", header.ScaleFactors.X)
            dataBuilder.AppendValue("FrameSize", "$*")
            AppendHeaderData(tr, dataBuilder, headerIds)
            tr.Commit()
        End Using
        dataBuilder.AddNewlyCreatedRow()
        Return dataBuilder.GetDataTable("ObjectID").Rows(0)
    End Function

    'small functions

    ''' <summary>
    ''' Gets the scale factor of the frame specified by the objectid.
    ''' <para>The frame is identified by the objectid of the blockreference representing that frame.</para>
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="frameId">The objectid of the frame.</param>
    ''' <returns>Scalefactor in text (eg '1:100').</returns>
    Public Shared Function GetScaleFactor(database As Database, frameId As ObjectId) As String
        Using tr = database.TransactionManager.StartTransaction()
            Return GetScaleFactor(tr, frameId)
            tr.Commit()
        End Using
    End Function

    ''' <summary>
    ''' Gets the scale factor of the frame specified by the objectid.
    ''' <para>The frame is identified by the objectid of the blockreference representing that frame.</para>
    ''' </summary>
    ''' <param name="transAction">The current transaction.</param>
    ''' <param name="frameId">The objectid of the frame.</param>
    ''' <returns>Scalefactor in text (eg '1:100').</returns>
    Public Shared Function GetScaleFactor(transAction As Transaction, frameId As ObjectId) As String
        Dim frame = transAction.GetBlockReference(frameId)
        If IsNothing(frame) Then Return "1:unknown"

        Dim btr = transAction.GetBlockTableRecord(frame.OwnerId)
        Dim layout = transAction.GetLayout(btr?.LayoutId)
        If IsNothing(layout) OrElse layout.LayoutName = "Model" Then
            Select Case frame.ScaleFactors.X < 1
                Case True : Return "{0}:1".Compose(Format(1 / frame.ScaleFactors.X, "#.#"))
                Case Else : Return "1:{0}".Compose(Format(frame.ScaleFactors.X, "#.#"))
            End Select
        End If

        Dim viewportIds = layout.GetViewports()
        If viewportIds.Count < 2 Then Return "1:1"

        Dim output = 1.0
        For i = 1 To viewportIds.Count - 1
            Dim viewport = transAction.GetViewport(viewportIds(i))
            Dim viewportScale = 1 / viewport.CustomScale
            Select Case True
                Case viewportScale < 1 Or viewportScale > 1000
                Case output < viewportScale : output = viewportScale
            End Select
        Next
        Return "1:" & Format(output, "#.#")
    End Function

    ''' <summary>
    ''' Creates a dictionary with the objectidcollections of the frames out of a drawing database.
    ''' <para>Each frame is identified by an <see cref="ObjectIdCollection"/> containing the objectids of the blockreferences representing the frame and its headers.</para>
    ''' </summary>
    ''' <param name="frameData">The frame database.</param>
    ''' <returns>The dictionary containing an <see cref="ObjectIdCollection"/> per frame.</returns>
    Public Shared Function GetFrameIdCollections(frameData As Data.DataTable) As Dictionary(Of String, ObjectIdCollection)
        Dim output = New Dictionary(Of String, ObjectIdCollection)
        For Each frameRow In frameData.Select
            output.TryAdd(frameRow.GetString("Num"), frameRow.GetValue("ObjectIDs"))
        Next
        Return output
    End Function

    ''' <summary>
    ''' Creates an empty drawinglist database.
    ''' </summary>
    ''' <returns>The database.</returns>
    Public Shared Function EmptyFramelist() As Data.DataTable
        Return ConvertToFramelist(New Data.DataTable)
    End Function

    ''' <summary>
    ''' Converts a drawing database to a drawinglist database.
    ''' </summary>
    ''' <param name="data">The database to convert.</param>
    ''' <returns>The converted database.</returns>
    Public Shared Function ConvertToFramelist(data As Data.DataTable) As Data.DataTable
        Dim columnNames = ("Filename;Num;Filedate;Dossier;Drawing;Sheet;Descr1;Descr2;Descr3;Descr4;Client1;Client2;Client3;Client4;Project;" &
            "Rev;FrameSize;Size;Scale;Design;Char;Date;Descr;Drawn;Check;LastRev_Char;LastRev_Date;LastRev_Descr;LastRev_Drawn;LastRev_Check").Cut
        For Each setting In columnNames
            If Not data.Columns.Contains(setting) Then data.Columns.Add(New Data.DataColumn(setting, GetType(String), "", MappingType.Attribute))
        Next
        Return New DataView(data).ToTable("Frames", False, columnNames)
    End Function

    'private functions

    ''' <summary>
    ''' Appends the texts in the header to the databuilder.
    ''' <para>Each frame is identified by an <see cref="ObjectIdCollection"/> containing the objectids of the blockreferences representing the frame and its headers.</para>
    ''' </summary>
    ''' <param name="transAction">The current tranaction.</param>
    ''' <param name="dataBuilder">The current databuilder.</param>
    ''' <param name="frameIds">The <see cref="ObjectIdCollection"/> containing the objectids of the frame and its headers.</param>
    ''' <returns>A boolean that indicates whether the attributes in the blocks can be traced back to drawingdata.</returns>
    Private Shared Function AppendHeaderData(transAction As Transaction, dataBuilder As DataBuilder, frameIds As ObjectIdCollection) As Boolean
        Dim allRevisions = New Dictionary(Of String, Dictionary(Of String, String))
        Dim lastRevision = ""
        Dim hasFrame = False
        Dim dataSet = DataSetHelper.LoadFromXml("{Support}\SetFramesInfo.xml".Compose)
        Dim frameInfoData = dataSet.GetTable("Frames", "Name")
        Dim headerInfoData = dataSet.GetTable("Headers", "Name")
        If IsNothing(frameInfoData) Or IsNothing(headerInfoData) Then Return False

        For i = 0 To frameIds.Count - 1
            Dim frameOrHeader = transAction.GetBlockReference(frameIds(i))
            If IsNothing(frameOrHeader) Then Continue For

            Dim family As String
            Select Case i
                Case 0 'is frame
                    Dim frameInfoRow = frameInfoData.Rows.Find(frameOrHeader.Name.Cut("$").First)
                    If IsNothing(frameInfoRow) Then Return False

                    hasFrame = True
                    dataBuilder.AppendValue("FrameSize", frameInfoRow.GetString("Frame"))
                    family = frameInfoRow.GetString("AttFam")
                Case Else
                    Dim headerInfoRow = headerInfoData.Rows.Find(frameOrHeader.Name)
                    If IsNothing(headerInfoRow) Then Continue For

                    family = headerInfoRow.GetString("AttFam")
            End Select
            Dim familyInfoData = dataSet.GetTable(family, "Name")
            If IsNothing(familyInfoData) Then Continue For

            Dim tags = New List(Of String)
            Dim attributeIds = frameOrHeader.AttributeCollection
            For j = 0 To attributeIds.Count - 1
                Dim attribute = transAction.GetAttributeReference(attributeIds(j))
                Dim tag = attribute.Tag
                Do While tags.Contains(tag)
                    tag = tag.AutoNumber
                Loop
                tags.Add(tag)
                Dim familyInfoRow = familyInfoData.Rows.Find(tag)
                If IsNothing(familyInfoRow) Then Continue For

                Dim infoTag = familyInfoRow.GetString("Info")
                If infoTag.StartsWith("Rev#") Then
                    Dim revisionTag = familyInfoRow.GetString("Revision")
                    allRevisions.TryAdd(infoTag, New Dictionary(Of String, String))
                    allRevisions(infoTag).TryAdd(revisionTag, attribute.TextString)
                    If revisionTag = "Date" Then
                        Dim revisionDate = DateStringHelper.Convert(attribute.TextString)
                        Select Case True
                            Case Not IsDate(revisionDate)
                            Case lastRevision = "" : lastRevision = infoTag
                            Case Not IsDate(allRevisions(lastRevision)("Date")) : lastRevision = infoTag
                            Case CDate(allRevisions(lastRevision)("Date")) < CDate(revisionDate) : lastRevision = infoTag
                        End Select
                    End If
                    infoTag &= "_{0}".Compose(revisionTag)
                End If
                dataBuilder.AppendValue(infoTag, attribute.TextString)
                dataBuilder.AppendValue("{0}_ID".Compose(infoTag), attribute.ObjectId)
            Next
        Next
        Select Case lastRevision = ""
            Case True : AppendLastRevision(dataBuilder, Nothing)
            Case Else : AppendLastRevision(dataBuilder, allRevisions(lastRevision))
        End Select

        If hasFrame Then
            dataBuilder.ChangeValue("Scale", GetScaleFactor(transAction, frameIds(0)))
            Dim frameSize = dataBuilder.GetValue("FrameSize")
            If NotNothing(frameSize) Then dataBuilder.ChangeValue("Size", frameSize)
        End If

        Return True
    End Function

    'private subs

    ''' <summary>
    ''' Appends the data of the last revision to the databuilder.
    ''' </summary>
    ''' <param name="dataBuilder">The current databuilder.</param>
    ''' <param name="dictionary">The dictionary containing the last revision data.</param>
    Private Shared Sub AppendLastRevision(ByRef dataBuilder As DataBuilder, dictionary As Dictionary(Of String, String))
        If IsNothing(dictionary) Then
            dataBuilder.AppendValue("LastRev_Date", "")
            dataBuilder.AppendValue("LastRev_Char", "0")
            dataBuilder.AppendValue("LastRev_Drawn", "")
            dataBuilder.AppendValue("LastRev_Descr", "")
            Exit Sub
        End If

        If dictionary.ContainsKey("Date") Then dataBuilder.AppendValue("LastRev_Date", dictionary("Date"))
        If dictionary.ContainsKey("Char") Then dataBuilder.AppendValue("LastRev_Char", dictionary("Char"))
        If dictionary.ContainsKey("Drawn") Then dataBuilder.AppendValue("LastRev_Drawn", dictionary("Drawn"))
        If dictionary.ContainsKey("Descr") Then dataBuilder.AppendValue("LastRev_Descr", dictionary("Descr"))
        If dictionary.ContainsKey("Check") Then dataBuilder.AppendValue("LastRev_Check", dictionary("Check"))
        If dictionary.ContainsKey("KOPREV") Then
            dataBuilder.AppendValue("LastRev_Char", dictionary("KOPREV").LeftString(1))
            Dim filter = " =;(=;)=".Cut.ToIniDictionary
            dataBuilder.AppendValue("LastRev_Drawn", dictionary("KOPREV").MidString(2).ReplaceMultiple(filter))
        End If
    End Sub

End Class
