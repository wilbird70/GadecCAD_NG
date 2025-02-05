'Gadec Engineerings Software (c) 2022
Imports System.Data
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.Geometry
Imports GadecCAD_NG.Extensions

''' <summary>
''' Provides methods for manipulating blockreferences.
''' </summary>
Public Class ReferenceHelper

    'subs

    ''' <summary>
    ''' Changes the scale of symbols (block references).
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="referenceIds">The objectids of block references.</param>
    ''' <param name="scale">The new scale.</param>
    Public Shared Sub ChangeScale(document As Document, referenceIds As ObjectIdCollection, scale As Double)
        Using document.LockDocument
            ChangeScale(document.Database, referenceIds, scale)
        End Using
    End Sub

    ''' <summary>
    ''' Changes the scale of symbols (block references).
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="referenceIds">The objectids of block references.</param>
    ''' <param name="scale">The new scale.</param>
    Public Shared Sub ChangeScale(database As Database, referenceIds As ObjectIdCollection, scale As Double)
        Using tr = database.TransactionManager.StartTransaction
            For Each referenceId In referenceIds
                Dim reference = tr.GetBlockReference(referenceId)
                Dim scaleFactor = scale / reference.ScaleFactors.X
                reference.UpgradeOpen()
                reference.TransformBy(Matrix3d.Scaling(scaleFactor, reference.Position))
                reference.DowngradeOpen()
            Next
            tr.Commit()
        End Using
    End Sub

    ''' <summary>
    ''' Changes the rotation of symbols (block references).
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="referenceIds">The objectids of block references.</param>
    ''' <param name="rotationInGrades">The rotation in grades (eg. 180 is half rotation).</param>
    Public Shared Sub ChangeRotation(document As Document, referenceIds As ObjectIdCollection, rotationInGrades As Double)
        Using document.LockDocument
            ChangeRotation(document.Database, referenceIds, rotationInGrades)
        End Using
    End Sub

    ''' <summary>
    ''' Changes the rotation of symbols (block references).
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="referenceIds">The objectids of block references.</param>
    ''' <param name="rotationInGrades">The rotation in grades (eg. 180 is half rotation).</param>
    Public Shared Sub ChangeRotation(database As Database, referenceIds As ObjectIdCollection, rotationInGrades As Double)
        Dim rotation = Math.PI * (rotationInGrades / 180)
        Using tr = database.TransactionManager.StartTransaction
            For Each referenceId In referenceIds
                Dim reference = tr.GetBlockReference(referenceId)
                Dim axisOfRotation = reference.BlockTransform.CoordinateSystem3d.Zaxis
                Dim rotationFactor = rotation - reference.Rotation
                reference.UpgradeOpen()
                reference.TransformBy(Matrix3d.Rotation(rotationFactor, axisOfRotation, reference.Position))
                reference.DowngradeOpen()
            Next
            tr.Commit()
        End Using
    End Sub

    'functions

    ''' <summary>
    ''' Explodes the last added entity (usually block references).
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <returns>A list of entities from the exploded entity.</returns>
    Public Shared Function CommandExplodeLast(document As Document) As Entity()
        Dim output = New List(Of Entity)
        Dim eventHandler As ObjectEventHandler = Sub(sender, e) If e.DBObject.GetDBObjectType = "BlockReference" Then output.Add(e.DBObject)
        With document
            AddHandler .Database.ObjectAppended, eventHandler
            .Editor.Command("EXPLODE", "L")
            RemoveHandler .Database.ObjectAppended, eventHandler
        End With
        Return output.ToArray
    End Function

    ''' <summary>
    ''' Explodes the block reference with the specified id.
    ''' <para>NOTE: After explode something goes wrong if groups are used.</para>
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="referenceId">The objectid of a block reference.</param>
    ''' <returns>A list of entities from the exploded block reference.</returns>
    Public Shared Function ExplodeToOwnerSpace(database As Database, referenceId As ObjectId) As Entity()
        Return ExplodeToOwnerSpace(database, New ObjectIdCollection({referenceId}))
    End Function

    ''' <summary>
    ''' Explodes the block references with the specified ids.
    ''' <para>NOTE: After explode something goes wrong if groups are used.</para>
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="referenceIds">The objectids of block references.</param>
    ''' <returns>A list of entities from the exploded block references.</returns>
    Public Shared Function ExplodeToOwnerSpace(database As Database, referenceIds As ObjectIdCollection) As Entity()
        Dim output = New List(Of Entity)
        Dim eventHandler As ObjectEventHandler = Sub(sender, e) If e.DBObject.GetDBObjectType = "BlockReference" Then output.Add(e.DBObject)

        Using tr = database.TransactionManager.StartTransaction
            For Each referenceId In referenceIds.ToArray
                Dim reference = tr.GetBlockReference(referenceId, OpenMode.ForWrite)
                AddHandler database.ObjectAppended, eventHandler
                reference.ExplodeToOwnerSpace()
                RemoveHandler database.ObjectAppended, eventHandler
                reference.Erase()
            Next
            tr.Commit()
        End Using
        Return output.ToArray
    End Function

    ''' <summary>
    ''' Explodes the block reference with the specified id.
    ''' <para>NOTE: Information about any used groups will be lost.</para>
    ''' <para>NOTE: Dynamic blocks lose their dynamic functionality.</para>
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="referenceId">The objectid of a block reference.</param>
    ''' <returns>A list of entities from the exploded block reference.</returns>
    Public Shared Function Explode(database As Database, referenceId As ObjectId) As Entity()
        Return Explode(database, New ObjectIdCollection({referenceId}))
    End Function

    ''' <summary>
    ''' Explodes the block references with the specified ids.
    ''' <para>NOTE: Information about any used groups will be lost.</para>
    ''' <para>NOTE: Dynamic blocks lose their dynamic functionality.</para>
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="referenceIds">The objectids of block references.</param>
    ''' <returns>A list of entities from the exploded block references.</returns>
    Public Shared Function Explode(database As Database, referenceIds As ObjectIdCollection) As Entity()
        Dim output = New List(Of Entity)
        Using tr = database.TransactionManager.StartTransaction
            For Each referenceId In referenceIds.Cast(Of ObjectId)
                Dim reference = tr.GetBlockReference(referenceId, OpenMode.ForWrite)
                Dim entities = New DBObjectCollection
                reference.Explode(entities)
                EntityHelper.Add(database, entities)
                output.AddRange(entities.Cast(Of Entity))
                reference.Erase()
            Next
            tr.Commit()
        End Using
        Return output.ToArray
    End Function

    ''' <summary>
    ''' Inserts a blockreference to the drawing of the specified definition, in the specified space and with the specified (optional) properties.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="spaceId">The objectid of the space (eg. Modelspace).</param>
    ''' <param name="layerId">The objectid of the layer.</param>
    ''' <param name="definitionId">The objectid of the definition (blocktablerecord).</param>
    ''' <param name="position">The insertionpoint for the blockreference.</param>
    ''' <param name="scale">The scalefactor for the blockreference.</param>
    ''' <param name="rotation">The rotation for the blockreference.</param>
    ''' <returns>The Objectid of the new blockreference.</returns>
    Public Shared Function InsertReference(document As Document, spaceId As ObjectId, layerId As ObjectId, definitionId As ObjectId, position As Point3d, Optional scale As Double = 1, Optional rotation As Double = 0) As ObjectId
        Using document.LockDocument
            Return InsertReference(document.Database, spaceId, layerId, definitionId, position, scale, rotation)
        End Using
    End Function

    ''' <summary>
    ''' Inserts a blockreference to the drawing of the specified definition, in the specified space and with the specified (optional) properties.
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="spaceId">The objectid of the space (eg. Modelspace).</param>
    ''' <param name="layerId">The objectid of the layer.</param>
    ''' <param name="definitionId">The objectid of the definition (blocktablerecord).</param>
    ''' <param name="position">The insertionpoint for the blockreference.</param>
    ''' <param name="scale">The scalefactor for the blockreference.</param>
    ''' <param name="rotation">The rotation for the blockreference.</param>
    ''' <returns>The objectid of the new blockreference.</returns>
    Public Shared Function InsertReference(database As Database, spaceId As ObjectId, layerId As ObjectId, definitionId As ObjectId, position As Point3d, Optional scale As Double = 1, Optional rotation As Double = 0) As ObjectId
        Dim reference = New BlockReference(position, definitionId) With {.LayerId = layerId, .ScaleFactors = New Scale3d(scale, scale, scale), .Rotation = rotation}
        Dim output = EntityHelper.Add(database, reference, spaceId)
        AddAttributesToReference(database, output)
        Return output
    End Function

    ''' <summary>
    ''' Gets almost all the data of a blockreference in a record.
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="referenceId">The objectid of a blockreference.</param>
    ''' <returns>A record with the reference data.</returns>
    Public Shared Function GetReferenceData(database As Database, referenceId As ObjectId) As DataRow
        Dim output = GetReferenceData(database, New ObjectIdCollection({referenceId}))
        Select Case output.Rows.Count > 0
            Case True : Return output.Rows(0)
            Case Else : Return Nothing
        End Select
    End Function

    ''' <summary>
    ''' Gets almost all the data of the blockreferences in a database (record per blockreference).
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="referenceIds">The objectids of the blockreferences.</param>
    ''' <returns>A database with the reference data.</returns>
    Public Shared Function GetReferenceData(database As Database, referenceIds As ObjectIdCollection) As Data.DataTable
        Dim dataBuilder = New DataBuilder("BlockReference", "BlockName;LayoutName".Cut)
        dataBuilder.InsertColumn("ObjectID", GetType(ObjectId), 0)
        Dim slaveReferencesIds = New List(Of ObjectId)
        Dim originalFileName = IO.Path.GetFileName(database.OriginalFileName)
        Using tr = database.TransactionManager.StartTransaction
            For Each referenceId In referenceIds.ToArray
                If referenceId.IsErased Then Continue For

                Dim reference = tr.GetBlockReference(referenceId)
                If IsNothing(reference) OrElse reference.OwnerId.IsErased Then Continue For

                Dim btr = tr.GetBlockTableRecord(reference.OwnerId)
                If Not btr.IsLayout Then Continue For

                Dim layout = tr.GetLayout(btr.LayoutId)
                Dim definitionId = If(reference.IsDynamicBlock, reference.DynamicBlockTableRecord, reference.BlockTableRecord)
                Dim definitionName = tr.GetBlockTableRecord(definitionId).Name

                dataBuilder.AppendValue("ObjectID", referenceId)
                dataBuilder.AppendValue("DefinitionID", definitionId)
                dataBuilder.AppendValue("OwnerID", reference.OwnerId)
                dataBuilder.AppendValue("BlockName", definitionName)
                dataBuilder.AppendValue("LayoutID", btr.LayoutId)
                dataBuilder.AppendValue("FileName", originalFileName)
                dataBuilder.AppendValue("LayoutName", layout.LayoutName)
                dataBuilder.AppendValue("Position", reference.Position)
                dataBuilder.AppendValue("ScaleFactors", reference.ScaleFactors)
                dataBuilder.AppendValue("Rotation", reference.Rotation)
                dataBuilder.AppendValue("BlockExtents", If(reference.Bounds.HasValue, reference.GeometricExtents, reference.GeometryExtentsBestFit))

                If reference.IsDynamicBlock Then
                    For Each referenceProperty In reference.DynamicBlockReferencePropertyCollection.ToArray
                        If referenceProperty.PropertyName = "Visibility1" Then
                            dataBuilder.AppendValue("Visibility", referenceProperty.Value)
                            Exit For
                        End If
                    Next
                End If
                Select Case True
                    Case definitionName.EndsWith("_F") : dataBuilder.AppendValue("SlaveBlock", True)
                    Case slaveReferencesIds.Contains(referenceId) : dataBuilder.AppendValue("SlaveBlock", True)
                    Case Else : slaveReferencesIds.AddRange(GroupHelper.GetGroupedEntityIds(database, referenceId).ToArray)
                End Select

                Dim attributeTags = New List(Of String)
                For Each attributeId In reference.AttributeCollection.ToArray
                    If attributeId.IsErased Then Continue For

                    Dim attribute = tr.GetAttributeReference(attributeId)
                    Dim attributeTag = attribute.Tag
                    Do While attributeTags.Contains(attributeTag)
                        attributeTag = attributeTag.AutoNumber
                    Loop
                    attributeTags.Add(attributeTag)
                    dataBuilder.AppendValue(attributeTag, attribute.TextString)
                    dataBuilder.AppendValue("{0}_ID".Compose(attributeTag), attribute.ObjectId)
                Next
                dataBuilder.AddNewlyCreatedRow()
            Next
            tr.Commit()
        End Using
        Return dataBuilder.GetDataTable("ObjectID")
    End Function

    ''' <summary>
    ''' Gets the extents of a blockreference.
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="referenceId">The objectid of a blockreference.</param>
    ''' <returns>The extents of the blockreference.</returns>
    Public Shared Function GetReferenceExtents(database As Database, referenceId As ObjectId) As Extents3d
        Dim output As Extents3d
        Using tr = database.TransactionManager.StartTransaction
            Dim reference = tr.GetBlockReference(referenceId)
            output = reference.GeometricExtents
            tr.Commit()
        End Using
        Return output
    End Function

    'private subs

    ''' <summary>
    ''' Adds the attributereferences from the blockdefinition to the blockreference.
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="referenceId">The objectid of a blockreference.</param>
    Private Shared Sub AddAttributesToReference(database As Database, referenceId As ObjectId)
        Try
            Using tr = database.TransactionManager.StartTransaction()
                Dim bt = tr.GetBlockTable(database.BlockTableId)
                Dim reference = tr.GetBlockReference(referenceId, OpenMode.ForWrite)
                Dim definition = tr.GetBlockTableRecord(bt(reference.Name))
                Dim attributeIds = reference.AttributeCollection

                For Each objectId In definition
                    Dim attributeDefinition = tr.GetAttributeDefinition(objectId)
                    If IsNothing(attributeDefinition) Then Continue For

                    Dim attribute = New AttributeReference
                    attribute.SetAttributeFromBlock(attributeDefinition, reference.BlockTransform)
                    attributeIds.AppendAttribute(attribute)
                    tr.AddNewlyCreatedDBObject(attribute, True)
                Next
                tr.Commit()
            End Using
        Catch ex As Exception
            ex.AddData($"ObjectId: {referenceId.ToString}")
            ex.Rethrow
        End Try
    End Sub

End Class
