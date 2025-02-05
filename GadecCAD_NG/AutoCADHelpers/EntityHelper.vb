'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports GadecCAD_NG.Extensions

''' <summary>
''' Provides methods for adding or deleting entities.
''' <para>Note: Also has an entity coloring method that also colors nested entities.</para>
''' </summary>
Public Class EntityHelper

    'add functions

    ''' <summary>
    ''' Adds an entity to the current space on the layer specified by the Gadec layertype from the current discipline.
    ''' <para>Note: The Gadec layertypes and disciplines are stored in the SetStandards.xml-file.</para>
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="entity">The entity to add.</param>
    ''' <param name="layerType">The type of the layer.</param>
    ''' <param name="lineTypeId">The objectid of a linetype.</param>
    ''' <returns></returns>
    Public Shared Function AddWithLayerType(document As Document, entity As DBObject, layerType As String, Optional lineTypeId As ObjectId = Nothing) As ObjectId
        Using document.LockDocument()
            Try
                Dim db = document.Database
                Dim layerId = If(layerType = "", db.Clayer, LayerHelper.GetLayerIdFromType(db, layerType))
                Return Add(db, New DBObjectCollection From {entity}, db.CurrentSpaceId, layerId, lineTypeId)(0)
            Catch ex As Exception
                ex.AddData($"layer: {layerType}")
                ex.Rethrow
                Return Nothing
            End Try
        End Using
    End Function

    ''' <summary>
    ''' Adds an entity to the current space.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="entity">The entity to add.</param>
    ''' <param name="layerId">The objectid of a layer.</param>
    ''' <param name="lineTypeId">The objectid of a linetype.</param>
    ''' <returns></returns>
    Public Shared Function Add(document As Document, entity As DBObject, Optional layerId As ObjectId = Nothing, Optional lineTypeId As ObjectId = Nothing) As ObjectId
        Using document.LockDocument()
            Return Add(document.Database, New DBObjectCollection From {entity}, document.Database.CurrentSpaceId, layerId, lineTypeId)(0)
        End Using
    End Function

    ''' <summary>
    ''' Adds entities to the current space.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="entities">The entities to add.</param>
    ''' <param name="layerId">The objectid of a layer.</param>
    ''' <param name="lineTypeId">The objectid of a linetype.</param>
    ''' <returns></returns>
    Public Shared Function Add(document As Document, entities As DBObjectCollection, Optional layerId As ObjectId = Nothing, Optional lineTypeId As ObjectId = Nothing) As ObjectIdCollection
        Using document.LockDocument()
            Return Add(document.Database, entities, document.Database.CurrentSpaceId, layerId, lineTypeId)
        End Using
    End Function

    ''' <summary>
    ''' Adds an entity to the drawing.
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="entity">The entity to add.</param>
    ''' <param name="spaceId">The objectid of the space to add to (eg. Modelspace).</param>
    ''' <param name="layerId">The objectid of a layer.</param>
    ''' <param name="lineTypeId">The objectid of a linetype.</param>
    ''' <returns></returns>
    Public Shared Function Add(database As Database, entity As DBObject, Optional spaceId As ObjectId = Nothing, Optional layerId As ObjectId = Nothing, Optional lineTypeId As ObjectId = Nothing) As ObjectId
        Return Add(database, New DBObjectCollection From {entity}, spaceId, layerId, lineTypeId)(0)
    End Function

    ''' <summary>
    ''' Adds entities to the drawing.
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="entities">The entities to add.</param>
    ''' <param name="spaceId">The objectid of the space to add to (eg. Modelspace).</param>
    ''' <param name="layerId">The objectid of a layer.</param>
    ''' <param name="lineTypeId">The objectid of a linetype.</param>
    ''' <returns></returns>
    Public Shared Function Add(database As Database, entities As DBObjectCollection, Optional spaceId As ObjectId = Nothing, Optional layerId As ObjectId = Nothing, Optional lineTypeId As ObjectId = Nothing) As ObjectIdCollection
        If spaceId.IsNull Then spaceId = database.CurrentSpaceId
        Dim output = New ObjectIdCollection
        Using tr = database.TransactionManager.StartTransaction()
            Dim btr = tr.GetBlockTableRecord(spaceId, OpenMode.ForWrite)
            For Each entity In entities.ToArray
                output.Add(btr.AppendEntity(entity))
                tr.AddNewlyCreatedDBObject(entity, True)
                If Not layerId.IsNull Then entity.LayerId = layerId
                If Not lineTypeId.IsNull Then entity.LinetypeId = lineTypeId
                If entity.GetDBObjectType = "DBText" Then entity.CastAsDBText.TextStyleId = TextStyleHelper.GetTextStyleId(database, "ISO_94")
            Next
            tr.Commit()
        End Using
        Return output
    End Function

    'delete functions

    ''' <summary>
    ''' Deletes the specified entity.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="entityId">The objectid of the entity to delete.</param>
    ''' <returns></returns>
    Public Shared Function Delete(document As Document, entityId As ObjectId) As Boolean
        Using document.LockDocument()
            Return Delete(document.Database, New ObjectIdCollection({entityId}))
        End Using
    End Function

    ''' <summary>
    ''' Deletes the specified entities.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="entityIds">The objectids of the entities to delete.</param>
    ''' <returns></returns>
    Public Shared Function Delete(document As Document, entityIds As ObjectIdCollection) As Boolean
        Using document.LockDocument()
            Return Delete(document.Database, entityIds)
        End Using
    End Function

    ''' <summary>
    ''' Deletes the specified entity.
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="entityId">The objectid of the entity to delete.</param>
    ''' <returns></returns>
    Public Shared Function Delete(database As Database, entityId As ObjectId) As Boolean
        Return Delete(database, New ObjectIdCollection({entityId}))
    End Function

    ''' <summary>
    ''' Deletes the specified entities.
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="entityIds">The objectids of the entities to delete.</param>
    ''' <returns></returns>
    Public Shared Function Delete(database As Database, entityIds As ObjectIdCollection) As Boolean
        Try
            Using tr = database.TransactionManager.StartTransaction()
                For Each entityId In entityIds.ToArray
                    Dim entity = tr.GetEntity(entityId, OpenMode.ForWrite, True)
                    If IsNothing(entity) Then Continue For

                    entity.Erase()
                Next
                tr.Commit()
            End Using
            Return True
        Catch ex As Exception
            ex.AddData($"ObjectIds: {String.Join(", ", entityIds)}")
            ex.Rethrow
            Return False
        End Try
    End Function

    Public Shared Function Hide(database As Database, entityId As ObjectId) As Boolean
        Try
            Using tr = database.TransactionManager.StartTransaction()
                Dim entity = tr.GetEntity(entityId, OpenMode.ForWrite, True)
                If NotNothing(entity) Then entity.Visible = False
                tr.Commit()
            End Using
            Return True
        Catch ex As Exception
            ex.AddData($"ObjectIds: {entityId.ToString}")
            ex.Rethrow
            Return False
        End Try
    End Function

    Public Shared Function Show(database As Database, entityId As ObjectId) As Boolean
        Try
            Using tr = database.TransactionManager.StartTransaction()
                Dim entity = tr.GetEntity(entityId, OpenMode.ForWrite, True)
                If NotNothing(entity) Then entity.Visible = True
                tr.Commit()
            End Using
            Return True
        Catch ex As Exception
            ex.AddData($"ObjectIds: {entityId.ToString}")
            ex.Rethrow
            Return False
        End Try
    End Function

End Class
