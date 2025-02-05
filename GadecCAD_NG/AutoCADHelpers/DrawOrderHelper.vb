'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.EditorInput
Imports GadecCAD_NG.Extensions

''' <summary>
''' Provides methods for changing the draworder of entities.
''' </summary>
Public Class DrawOrderHelper

    'bringtofront

    ''' <summary>
    ''' Brings entities on the specified layer to front.
    ''' <para>Note: If there are blockreferences on that layer, they will become on top of other entities.</para>
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="layerName">The name of the layer.</param>
    Public Shared Sub BringToFront(document As Document, layerName As String)
        Dim db = document.Database
        Dim filter = New SelectionFilter({New TypedValue(DxfCode.LayerName, layerName), New TypedValue(DxfCode.LayoutName, LayoutHelper.GetCurrentName)})
        Dim selectionResult = document.Editor.SelectAll(filter)
        If Not selectionResult.Status = PromptStatus.OK Then Exit Sub

        Using document.LockDocument
            BringToFront(db, db.CurrentSpaceId, selectionResult.Value.GetObjectIds)
            document.Editor.Regen()
        End Using
    End Sub

    ''' <summary>
    ''' Brings entities with the specified objectids to front.
    ''' <para>Note: If there are blockreferences with them, they will become on top of other entities.</para>
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="entityIds">The objectids of the entities.</param>
    Public Shared Sub BringToFront(document As Document, entityIds As ObjectIdCollection)
        Using document.LockDocument
            BringToFront(document.Database, document.Database.CurrentSpaceId, entityIds.ToArray)
            document.Editor.Regen()
        End Using
    End Sub

    ''' <summary>
    ''' Brings an entity with the specified objectid to front.
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="spaceId">The objectid of the space (eg. Modelspace).</param>
    ''' <param name="entityId">The objectid of the entity.</param>
    Public Shared Sub BringToFront(database As Database, spaceId As ObjectId, entityId As ObjectId)
        BringToFront(database, spaceId, {entityId})
    End Sub

    ''' <summary>
    ''' Brings entities with the specified objectids to front.
    ''' <para>Note: If there are blockreferences with them, they will become on top of other entities.</para>
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="spaceId">The objectid of the space (eg. Modelspace).</param>
    ''' <param name="entityIds">The objectids of the entities.</param>
    Public Shared Sub BringToFront(database As Database, spaceId As ObjectId, entityIds As ObjectIdCollection)
        BringToFront(database, spaceId, entityIds.ToArray)
    End Sub

    'sendtoback

    ''' <summary>
    ''' Sends entities on the specified layer to back.
    ''' <para>Note: If there are blockreferences on that layer, they will become on top of other entities.</para>
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="layerName">The name of the layer.</param>
    Public Shared Sub SendToBack(document As Document, layerName As String)
        Dim db = document.Database
        Dim filter = New SelectionFilter({New TypedValue(DxfCode.LayerName, layerName), New TypedValue(DxfCode.LayoutName, LayoutHelper.GetCurrentName)})
        Dim selectionResult = document.Editor.SelectAll(filter)
        If Not selectionResult.Status = PromptStatus.OK Then Exit Sub

        Using document.LockDocument
            SendToBack(db, db.CurrentSpaceId, selectionResult.Value.GetObjectIds)
            document.Editor.Regen()
        End Using
    End Sub

    ''' <summary>
    ''' Sends entities with the specified objectids to back.
    ''' <para>Note: If there are blockreferences with them, they will become on top of other entities.</para>
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="entityIds">The objectids of the entities.</param>
    Public Shared Sub SendToBack(document As Document, entityIds As ObjectIdCollection)
        Using document.LockDocument
            SendToBack(document.Database, document.Database.CurrentSpaceId, entityIds.ToArray)
            document.Editor.Regen()
        End Using
    End Sub

    ''' <summary>
    ''' Sends an entity with the specified objectid to back.
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="spaceId">The objectid of the space (eg. Modelspace).</param>
    ''' <param name="entityId">The objectid of the entity.</param>
    Public Shared Sub SendToBack(database As Database, spaceId As ObjectId, entityId As ObjectId)
        SendToBack(database, spaceId, {entityId})
    End Sub

    ''' <summary>
    ''' Sends entities with the specified objectids to back.
    ''' <para>Note: If there are blockreferences with them, they will become on top of other entities.</para>
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="spaceId">The objectid of the space (eg. Modelspace).</param>
    ''' <param name="entityIds">The objectids of the entities.</param>
    Public Shared Sub SendToBack(database As Database, spaceId As ObjectId, entityIds As ObjectIdCollection)
        SendToBack(database, spaceId, entityIds.ToArray)
    End Sub

    'private subs

    ''' <summary>
    ''' Brings entities with the specified objectids to front.
    ''' <para>Note: If there are blockreferences with them, they will become on top of other entities.</para>
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="spaceId">The objectid of the space (eg. Modelspace).</param>
    ''' <param name="entityIds">The objectids of the entities.</param>
    Private Shared Sub BringToFront(database As Database, spaceId As ObjectId, entityIds As ObjectId())
        ChangeDrawOrder(database, spaceId, entityIds, "BringToFront")
    End Sub

    ''' <summary>
    ''' Sends entities with the specified objectids to back.
    ''' <para>Note: If there are blockreferences with them, they will become on top of other entities.</para>
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="spaceId">The objectid of the space (eg. Modelspace).</param>
    ''' <param name="entityIds">The objectids of the entities.</param>
    Private Shared Sub SendToBack(database As Database, spaceId As ObjectId, entityIds As ObjectId())
        ChangeDrawOrder(database, spaceId, entityIds, "SendToBack")
    End Sub

    ''' <summary>
    ''' Changes the draworder of entities with the specified objectids.
    ''' <para>Note: If there are blockreferences with them, they will become on top of other entities.</para>
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="spaceId">The objectid of the space (eg. Modelspace).</param>
    ''' <param name="entityIds">The objectids of the entities.</param>
    ''' <param name="actionString">The action to perform: 'BringToFront' or 'SendToBack'.</param>
    Private Shared Sub ChangeDrawOrder(database As Database, spaceId As ObjectId, entityIds As ObjectId(), actionString As String)
        If entityIds.Count = 0 Then Exit Sub

        Using tr = database.TransactionManager.StartTransaction
            Dim btr = tr.GetBlockTableRecord(spaceId)
            Dim drawOrderTable = tr.GetDrawOrderTable(btr.DrawOrderTableId, OpenMode.ForWrite)
            Dim entities = New Dictionary(Of Boolean, ObjectIdCollection) From {{True, New ObjectIdCollection}, {False, New ObjectIdCollection}}
            entityIds.ToList.ForEach(Sub(id) entities(id.ObjectClass.DxfName.ToLower = "insert").Add(id))
            Select Case actionString
                Case "BringToFront"
                    If entities(False).Count > 0 Then drawOrderTable.MoveToTop(entities(False))
                    If entities(True).Count > 0 Then drawOrderTable.MoveToTop(entities(True))
                Case "SendToBack"
                    If entities(True).Count > 0 Then drawOrderTable.MoveToBottom(entities(True))
                    If entities(False).Count > 0 Then drawOrderTable.MoveToBottom(entities(False))
            End Select
            tr.Commit()
        End Using
    End Sub

End Class
