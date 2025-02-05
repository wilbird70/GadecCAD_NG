'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports GadecCAD_NG.Extensions

''' <summary>
''' <para><see cref="DocumentEvents"/> adds several events to the <see cref="Document"/> required by the application.</para>
''' </summary>
Public Class DocumentEvents
    Implements IDisposable
    Private _disposed As Boolean

    ''' <summary>
    ''' The reference to the present <see cref="Document"/>.
    ''' </summary>
    Private ReadOnly _thisDocument As Document
    ''' <summary>
    ''' Contains all ObjectIdCollections to track changes on blockreferences.
    ''' </summary>
    Private _modifiedObjects As New ModifiedObjects
    Private _modifiedFields As Boolean = False

    ''' <summary>
    ''' Initializes a new instance of <see cref="DocumentEvents"/>.
    ''' <para><see cref="DocumentEvents"/> adds several events to the <see cref="Document"/> required by the application.</para>
    ''' </summary>
    ''' <param name="document">The <see cref="Document"/> for which to create the events.</param>
    Public Sub New(document As Document)
        _thisDocument = document
        With _thisDocument
            AddHandler .CommandCancelled, AddressOf CommandCancelledEventHandler
        End With
        With _thisDocument.Editor
            AddHandler .EnteringQuiescentState, AddressOf EnteringQuiescentStateEventHandler
        End With
        With _thisDocument.Database
            AddHandler .ObjectAppended, AddressOf ObjectAppendedEventHandler
            AddHandler .ObjectUnappended, AddressOf ObjectUnappendedEventHandler
            AddHandler .ObjectReappended, AddressOf ObjectReappendedEventHandler
            AddHandler .ObjectErased, AddressOf ObjectErasedEventHandler
            AddHandler .ObjectModified, AddressOf ObjectModifiedEventHandler
            AddHandler .BeginSave, AddressOf BeginSaveEventHandler
            AddHandler .AbortSave, AddressOf AbortSaveEventHandler
            AddHandler .SaveComplete, AddressOf SaveCompleteEventHandler
        End With
    End Sub

    ''' <summary>
    ''' Removes the <see cref="Document"/> events and empty the objectidcollections when disposing.
    ''' </summary>
    ''' <param name="disposing"></param>
    Protected Overridable Sub Dispose(disposing As Boolean)
        If _disposed Then Exit Sub

        If disposing Then
            With _thisDocument
                RemoveHandler .CommandCancelled, AddressOf CommandCancelledEventHandler
            End With
            With _thisDocument.Editor
                RemoveHandler .EnteringQuiescentState, AddressOf EnteringQuiescentStateEventHandler
            End With
            With _thisDocument.Database
                RemoveHandler .ObjectAppended, AddressOf ObjectAppendedEventHandler
                RemoveHandler .ObjectUnappended, AddressOf ObjectUnappendedEventHandler
                RemoveHandler .ObjectReappended, AddressOf ObjectReappendedEventHandler
                RemoveHandler .ObjectErased, AddressOf ObjectErasedEventHandler
                RemoveHandler .ObjectModified, AddressOf ObjectModifiedEventHandler
                RemoveHandler .BeginSave, AddressOf BeginSaveEventHandler
                RemoveHandler .AbortSave, AddressOf AbortSaveEventHandler
                RemoveHandler .SaveComplete, AddressOf SaveCompleteEventHandler
            End With
            _modifiedObjects = Nothing
        End If
        _disposed = True
    End Sub

    ''' <summary>
    ''' Implements <see cref="IDisposable.Dispose"/>.
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
        Dispose(disposing:=True)
        GC.SuppressFinalize(Me)
    End Sub

    'subs

    ''' <summary>
    ''' Processing frames that may have changed.
    ''' </summary>
    Public Sub ProcessFrameChanges()
        If _modifiedObjects.ChangedFrameIds.Count = 0 Then Exit Sub

        ChangeFramesInFrameData(_modifiedObjects.ChangedFrameIds)
        _modifiedObjects.ChangedFrameIds.Clear()
    End Sub

    ''' <summary>
    ''' Process symbols that may have changed.
    ''' </summary>
    Public Sub ProcessSymbolChanges(symbolIds As ObjectIdCollection)
        Dim symbolData = ReferenceHelper.GetReferenceData(_thisDocument.Database, symbolIds)
        Dim textToChange = New Dictionary(Of ObjectId, String)
        For Each symbolRow In symbolData.Select
            Select Case True
                Case Not symbolRow.HasValue("WNGD")
                Case symbolRow.HasValue("LS")
                    Select Case True
                        Case Not symbolRow.HasValue("ADR")
                        Case Not symbolRow.HasValue("PNR")
                        Case Not symbolRow.HasValue("TYPE")
                        Case Else
                            Select Case True
                                Case symbolRow.GetString("Visibility") = symbolRow.GetString("TYPE")
                                Case Not symbolRow.GetString("Visibility") = "" : textToChange.TryAdd(symbolRow.GetAttributeId("TYPE"), symbolRow.GetString("Visibility"))
                            End Select
                            Dim winguard = "{0}.{1}{2}".Compose(symbolRow.GetString("PNR"), symbolRow.GetString("LS"), symbolRow.GetString("ADR"))
                            textToChange.TryAdd(symbolRow.GetAttributeId("WNGD"), If(winguard = ".", "", winguard))
                            If symbolRow.HasValue("DOT") Then textToChange.TryAdd(symbolRow.GetAttributeId("DOT"), If(winguard = ".", "", "/"))
                    End Select
                Case symbolRow.HasValue("ZONENR")
                    Select Case True
                        Case Not symbolRow.HasValue("LAND")
                        Case Not symbolRow.HasValue("WINKEL")
                        Case Not symbolRow.HasValue("POSTCODE")
                        Case Else
                            Dim winguard = "{0}.{1}.{2}.{3}".Compose(symbolRow.GetString("LAND"), symbolRow.GetString("WINKEL"), symbolRow.GetString("POSTCODE"), symbolRow.GetString("ZONENR"))
                            textToChange.TryAdd(symbolRow.GetAttributeId("WNGD"), If(winguard = "...", "", winguard))
                    End Select
            End Select
        Next
        _ObjectModifiedEnabled = False  'ObjectModified-event even uit
        TextHelper.ChangeTextStrings(_thisDocument, textToChange)
        _ObjectModifiedEnabled = True   'ObjectModified-event weer aan
    End Sub

    'private eventhandlers

    ''' <summary>
    ''' EventHandler for the event that occurs when an object is added.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ObjectAppendedEventHandler(sender As Object, e As ObjectEventArgs)
        If Not _DocumentEventsEnabled Then Exit Sub

        ObjectAddedEventHandler(e.DBObject)
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when an object disappears by the undo command.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ObjectUnappendedEventHandler(sender As Object, e As ObjectEventArgs)
        If Not _DocumentEventsEnabled Then Exit Sub

        ObjectDeletedEventHandler(e.DBObject)
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when an object reappears by the redo command.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ObjectReappendedEventHandler(sender As Object, e As ObjectEventArgs)
        If Not _DocumentEventsEnabled Then Exit Sub

        ObjectAddedEventHandler(e.DBObject)
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when an object is erased.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ObjectErasedEventHandler(sender As Object, e As ObjectErasedEventArgs)
        If Not _DocumentEventsEnabled Then Exit Sub

        If e.Erased Then
            ObjectDeletedEventHandler(e.DBObject)
        Else
            ObjectAddedEventHandler(e.DBObject)
        End If
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when user cancels a command.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub CommandCancelledEventHandler(sender As Object, e As CommandEventArgs)
        If Not _DocumentEventsEnabled Then Exit Sub

        _CommandCanceled = True
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when an object in the drawing is modified.
    ''' <para>Tracks frames and symbols being modified.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ObjectModifiedEventHandler(sender As Object, e As ObjectEventArgs)
        Try
            If Not _DocumentEventsEnabled Or Not _ObjectModifiedEnabled Then Exit Sub

            Dim referenceId As ObjectId
            Select Case e.DBObject.GetDBObjectType
                Case "BlockReference"
                    referenceId = e.DBObject.ObjectId
                    If _modifiedObjects.ChangedBlockReferenceIds.Contains(referenceId) Then Exit Sub

                    _modifiedObjects.ChangedBlockReferenceIds.Add(referenceId)
                Case "AttributeReference"

                    If e.DBObject.CastAsAttributeReference.HasFields Then _modifiedFields = True

                    referenceId = e.DBObject.OwnerId
                    If _modifiedObjects.ChangedAttributeOwnerIds.Contains(referenceId) Then Exit Sub

                    Select Case e.DBObject.CastAsAttributeReference.Tag
                        Case "LS", "ADR", "PNR", "WNGD", "TYPE"
                            _modifiedObjects.ChangedAttributeOwnerIds.Add(referenceId)
                            _modifiedObjects.ChangedSymbolIds.Add(referenceId)
                    End Select
                Case Else : Exit Sub
            End Select
            For Each frameRow In _thisDocument.FrameData.Select
                Dim frameId = frameRow.GetObjectId("ObjectID")
                If _modifiedObjects.ChangedFrameIds.Contains(frameId) Then Continue For

                Dim frameIds = frameRow.GetObjectIdCollection("ObjectIDs")
                Select Case True
                    Case frameId = referenceId : _modifiedObjects.ChangedFrameIds.Add(frameId)
                    Case frameIds.Contains(referenceId) : _modifiedObjects.ChangedFrameIds.Add(frameId)
                End Select
            Next
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when user wants to save the document.
    ''' <para>While saving the object modified events will be disabled.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub BeginSaveEventHandler(sender As Object, e As DatabaseIOEventArgs)
        If Not _DocumentEventsEnabled Then Exit Sub

        _ObjectModifiedEnabled = False 'ObjectModified-event tijdelijk uit
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when user wants to save the document, but changes his mind in time.
    ''' <para>After abortion the object modified events will be enabled again.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub AbortSaveEventHandler(sender As Object, e As EventArgs)
        If Not _DocumentEventsEnabled Then Exit Sub

        _ObjectModifiedEnabled = True 'ObjectModified-event weer aan
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when saving the document is complete.
    ''' <para>This handler controls:</para>
    ''' <para>- Reloading the framelist on the GADEC palette.</para>
    ''' <para>- Reloading the filelist on the GADEC palette.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub SaveCompleteEventHandler(sender As Object, e As DatabaseIOEventArgs)
        Try
            If Not _DocumentEventsEnabled Then Exit Sub

            _ObjectModifiedEnabled = True 'ObjectModified-event weer aan
            Dim frameSetController = New FrameSetHandler(e.FileName, True)
            Dim path = IO.Path.GetDirectoryName(e.FileName)
            PrefixLengths.TryAdd(path, -1)
            If _thisDocument.GetPath = "" Then Exit Sub

            PaletteHelper.ReloadFrameList()
            PaletteHelper.ReloadFileList()
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when a command is completed or aborted and the command line is ready to receive a new command.
    ''' <para>This handler will process:</para>
    ''' <para>- The symbols being modified.</para>
    ''' <para>- The frames being added.</para>
    ''' <para>- The frames being removed.</para>
    ''' <para>- The frames being modified.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub EnteringQuiescentStateEventHandler(sender As Object, e As EventArgs)
        Try
            If _modifiedFields Then _modifiedFields = False : _thisDocument.Editor.Regen()

            If _modifiedObjects.ChangedSymbolIds.Count > 0 Then ProcessSymbolChanges(_modifiedObjects.ChangedSymbolIds)
            If _modifiedObjects.AddedFrameIds.Count > 0 Then AddFramesInFrameData(_modifiedObjects.AddedFrameIds)
            If _modifiedObjects.DeletedFrameIds.Count > 0 Then DeleteFramesFromFrameData(_modifiedObjects.DeletedFrameIds)
            If _modifiedObjects.ChangedFrameIds.Count > 0 Then ChangeFramesInFrameData(_modifiedObjects.ChangedFrameIds)
            _modifiedObjects = New ModifiedObjects
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    'private subs

    ''' <summary>
    ''' EventHandler for the event that occurs when an object is added or re-added to the drawing.
    ''' <para>Tracks frames being added.</para>
    ''' </summary>
    ''' <param name="entity"></param>
    Private Sub ObjectAddedEventHandler(entity As DBObject)
        Try
            If Not _DocumentEventsEnabled Then Exit Sub

            Dim referenceId As ObjectId
            Select Case entity.GetDBObjectType
                Case "BlockReference"
                    referenceId = entity.ObjectId
                    If _modifiedObjects.ChangedBlockReferenceIds.Contains(referenceId) Then Exit Sub

                    _modifiedObjects.ChangedBlockReferenceIds.Add(referenceId)
                    If _modifiedObjects.AddedFrameIds.Contains(referenceId) Then Exit Sub

                    Dim frameName = entity.CastAsBlockReference.Name.Cut("$").First
                    If IsNothing(_frameNames) Then _frameNames = GetFrameNames()
                    If Not _frameNames.Contains(frameName) Then Exit Sub

                    _modifiedObjects.AddedFrameIds.Add(referenceId)
                Case "AttributeReference"
                    referenceId = entity.OwnerId
                    If _modifiedObjects.ChangedAttributeOwnerIds.Contains(referenceId) Then Exit Sub

                    _modifiedObjects.ChangedAttributeOwnerIds.Add(referenceId)
            End Select
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when an object is removed from the drawing.
    ''' <para>Tracks frames being removed.</para>
    ''' </summary>
    ''' <param name="entity"></param>
    Private Sub ObjectDeletedEventHandler(entity As DBObject)
        Try
            If Not _DocumentEventsEnabled Then Exit Sub
            If Not entity.GetDBObjectType = "BlockReference" Then Exit Sub

            If Not _modifiedObjects.DeletedFrameIds.Contains(entity.ObjectId) Then
                Dim frameRow = _thisDocument.FrameData.Rows.Find(entity.ObjectId)
                If NotNothing(frameRow) Then _modifiedObjects.DeletedFrameIds.Add(entity.ObjectId)
            End If
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' Process frames being added and reloading the framelist on the GADEC palette.
    ''' </summary>
    ''' <param name="frameIds"></param>
    Private Sub AddFramesInFrameData(frameIds As ObjectIdCollection)
        Dim frameData = _thisDocument.FrameData
        Dim appendedFrameData = FrameHelper.BuildFrameData(_thisDocument, frameIds)
        frameData.Merge(appendedFrameData)
        Dim frameIdCollections = FrameHelper.GetFrameIdCollections(_thisDocument.FrameData)
        XRecordObjectIdsHelper.Update(_thisDocument, "{Company}".Compose, "FrameWorkIDs", frameIdCollections)
        frameIds.Clear()
        PaletteHelper.ReloadFrameList()
        If Not appendedFrameData.Rows.Count = 1 Then Exit Sub

        Dim appendedFrameRow = appendedFrameData.Rows(0)
        FrameViewHelper.SetView(_thisDocument, appendedFrameRow.GetValue("ObjectID"))
        If appendedFrameRow.GetString("LayoutName") = "Model" Then Exit Sub

        Dim file = If(_thisDocument.IsNamedDrawing, "{0}\{1}", "{1}").Compose(_thisDocument.GetPath, appendedFrameRow.GetString("Filename"))
        Dim number = appendedFrameRow.GetString("Num")
        Dim rowSelection = New Dictionary(Of String, String) From {{file, number}}
        Dim framePlotter = New FramePlotter(rowSelection, "ToPDF")
        framePlotter.SetPlotLayout()
    End Sub

    ''' <summary>
    ''' Process frames being removed and reloading the framelist on the GADEC palette.
    ''' </summary>
    ''' <param name="frameIds"></param>
    Private Sub DeleteFramesFromFrameData(frameIds As ObjectIdCollection)
        Dim frameData = _thisDocument.FrameData
        For Each objectId In frameIds.ToArray
            frameData.Rows.Find(objectId)?.Delete()
        Next
        Dim frameIdCollections = FrameHelper.GetFrameIdCollections(_thisDocument.FrameData)
        XRecordObjectIdsHelper.Update(_thisDocument, "{Company}".Compose, "FrameWorkIDs", frameIdCollections)
        frameIds.Clear()
        PaletteHelper.ReloadFrameList()
    End Sub

    ''' <summary>
    ''' Process frames being modified and reloading the framelist on the GADEC palette.
    ''' </summary>
    ''' <param name="frameIds"></param>
    Private Sub ChangeFramesInFrameData(frameIds As ObjectIdCollection)
        Dim frameData = _thisDocument.FrameData
        For Each objectId In frameIds.ToArray
            frameData.Rows.Find(objectId)?.Delete()
        Next
        Dim changedFrameData = FrameHelper.BuildFrameData(_thisDocument, frameIds)
        frameData.Merge(changedFrameData)
        Dim frameIdCollections = FrameHelper.GetFrameIdCollections(_thisDocument.FrameData)
        XRecordObjectIdsHelper.Update(_thisDocument, "{Company}".Compose, "FrameWorkIDs", frameIdCollections)
        frameIds.Clear()
        PaletteHelper.ReloadFrameList()
    End Sub

    '///shared part of class\\\

    ''' <summary>
    ''' List that contains all the framenames loaded from the 'SetFramesInfo.xml' file.
    ''' <para>To check if blockreferences are a frame.</para>
    ''' </summary>
    Private Shared _frameNames As List(Of String)

    ''' <summary>
    ''' Property to control the availability of the document events.
    ''' <para>For example, disable while running scripts.</para>
    ''' </summary>
    ''' <returns></returns>
    Public Shared Property DocumentEventsEnabled As Boolean = True
    ''' <summary>
    ''' Property to control the availability of the events that occurs when (drawing) objects are added, erased or modified.
    ''' <para>For example, disable while executing commands that creates or modifies a lot of entities.</para>
    ''' </summary>
    ''' <returns></returns>
    Public Shared Property ObjectModifiedEnabled As Boolean = True
    ''' <summary>
    ''' Property that becomes True when a command is canceled.
    ''' <para>Note: For use set to False first.</para>
    ''' </summary>
    ''' <returns></returns>
    Public Shared Property CommandCanceled As Boolean

    'private functions

    ''' <summary>
    ''' Loads all the framenames from the 'SetFramesInfo.xml' file.
    ''' </summary>
    ''' <returns></returns>
    Private Shared Function GetFrameNames() As List(Of String)
        Dim frameInfoData = DataSetHelper.LoadFromXml("{Support}\SetFramesInfo.xml".Compose).GetTable("Frames", "Name") 'done
        If IsNothing(frameInfoData) Then Return Nothing

        Return frameInfoData.GetStringsFromColumn("Name").ToList
    End Function

End Class
