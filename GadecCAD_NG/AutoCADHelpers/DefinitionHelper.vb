'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports GadecCAD_NG.Extensions

''' <summary>
''' Provides methods for manipulating blockdefinitions.
''' </summary>
Public Class DefinitionHelper

    'subs

    ''' <summary>
    ''' Purges the block definition with the specified objectid.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="definitionId">The objectid of a block definition (blocktablerecord).</param>
    Public Shared Sub PurgeBlock(document As Document, definitionId As ObjectId)
        Using document.LockDocument
            PurgeBlocks(document.Database, New ObjectIdCollection({definitionId}))
        End Using
    End Sub

    ''' <summary>
    ''' Purges the block definitions with the specified objectids.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="definitionIds">The objectids of block definitions (blocktablerecords).</param>
    Public Shared Sub PurgeBlocks(document As Document, definitionIds As ObjectIdCollection)
        Using document.LockDocument
            PurgeBlocks(document.Database, definitionIds)
        End Using
    End Sub

    ''' <summary>
    ''' Purges the block definition with the specified objectid.
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="definitionId">The objectid of a block definition (blocktablerecord).</param>
    Public Shared Sub PurgeBlock(database As Database, definitionId As ObjectId)
        PurgeBlocks(database, New ObjectIdCollection({definitionId}))
    End Sub

    ''' <summary>
    ''' Purges the block definitions with the specified objectids.
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="definitionIds">The objectids of block definitions (blocktablerecords).</param>
    Public Shared Sub PurgeBlocks(database As Database, definitionIds As ObjectIdCollection)
        If definitionIds.Count = 0 Then Exit Sub

        Using tr = database.TransactionManager.StartTransaction
            For Each definitionId In definitionIds.ToArray
                If definitionId.IsErased Then Continue For

                Dim btr = tr.GetBlockTableRecord(definitionId, OpenMode.ForWrite)
                If btr.IsLayout OrElse btr.GetBlockReferenceIds(True, True).Count > 0 Then Continue For

                btr.Erase()
            Next
            tr.Commit()
        End Using
    End Sub

    ''' <summary>
    ''' Sends wipeouts within blockdefinitions of the specified blockreferences (by objectids) to the background (Z-order).
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="referenceIds">The objectids of blockreferences.</param>
    Public Shared Sub SendToBackWipeouts(database As Database, referenceIds As ObjectIdCollection)
        Dim definitionIds = GetDefinitionIds(database, referenceIds)
        SendToBackWipeouts(database, definitionIds)
    End Sub

    'functions

    ''' <summary>
    ''' Creates a new definition (blocktablerecord).
    ''' <para>Note: If a definition with the same blockname exists, it returns the objectid of that existing definition.</para>
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="entities">A list of entities to place in the definition.</param>
    ''' <param name="blockName">The blockName for the new definition.</param>
    ''' <returns>The objectid of the new definition (blocktablerecord).</returns>
    Public Shared Function CreateDefinition(database As Database, entities() As Entity, blockName As String) As ObjectId
        Dim output = ObjectId.Null
        Using tr = database.TransactionManager.StartTransaction()
            Dim bt = tr.GetBlockTable(database.BlockTableId, OpenMode.ForWrite)
            Select Case bt.Has(blockName)
                Case True : output = bt(blockName)
                Case Else
                    Dim definition = New BlockTableRecord With {.Name = blockName}
                    output = bt.Add(definition)
                    tr.AddNewlyCreatedDBObject(definition, True)
                    For Each entity In entities
                        definition.AppendEntity(entity)
                        tr.AddNewlyCreatedDBObject(entity, True)
                    Next
            End Select
            tr.Commit()
        End Using
        Return output
    End Function

    ''' <summary>
    ''' Adds all anonymousnames (of the other appearances) to the list of blocknames of dynamic blockdefinitions.
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="blockNames">A list of blocknames.</param>
    ''' <returns>The appended list of blocknames.</returns>
    Public Shared Function AddAnonymousNames(database As Database, blockNames As String()) As String()
        Dim output = New List(Of String)
        Using tr = database.TransactionManager.StartTransaction
            Dim bt = tr.GetBlockTable(database.BlockTableId)
            For Each blockName In blockNames
                output.Add(blockName)
                If Not bt.Has(blockName) Then Continue For

                Dim definition = tr.GetBlockTableRecord(bt(blockName))
                If Not definition.IsDynamicBlock Then Continue For

                For Each anonymousDefinitionId In definition.GetAnonymousBlockIds.ToArray
                    Dim anonymousDefinition = tr.GetBlockTableRecord(anonymousDefinitionId)
                    output.Add(If(anonymousDefinition.Name.StartsWith("*"), "`{0}".Compose(anonymousDefinition.Name), anonymousDefinition.Name))
                Next
            Next
            tr.Commit()
        End Using
        Return output.ToArray
    End Function

    'private functions

    ''' <summary>
    ''' Gets the definitionids (also the anonymous ones) of the specified blockreferences (by objectids).
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="referenceIds">The objectids of blockreferences.</param>
    ''' <returns></returns>
    Private Shared Function GetDefinitionIds(database As Database, referenceIds As ObjectIdCollection) As ObjectId()
        Dim output = New List(Of ObjectId)
        Using tr = database.TransactionManager.StartTransaction()
            For Each objectId In referenceIds.ToArray
                Dim reference = tr.GetBlockReference(objectId)
                If IsNothing(reference) Then Continue For

                Dim definitionId = If(reference.IsDynamicBlock, reference.DynamicBlockTableRecord, reference.BlockTableRecord)
                If output.Contains(definitionId) Then Continue For

                output.Add(definitionId)
                If Not reference.IsDynamicBlock Then Continue For

                Dim definition = tr.GetBlockTableRecord(definitionId)
                output.AddRange(definition.GetAnonymousBlockIds.ToArray)
            Next
            tr.Commit()
        End Using
        Return output.ToArray()
    End Function

    ''' <summary>
    ''' Sends wipeouts within specified blockdefinitions (by objectids) to the background (Z-order).
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="definitionIds">The objectids of blockdefinitions.</param>
    Private Shared Sub SendToBackWipeouts(database As Database, definitionIds As ObjectId())
        Dim referenceIds = New List(Of ObjectId)
        Using tr = database.TransactionManager.StartTransaction()
            For Each definitionId In definitionIds
                Dim btr = tr.GetBlockTableRecord(definitionId)
                Dim wipeouts = New ObjectIdCollection()
                btr.ToList.ForEach(Sub(id) If id.ObjectClass.DxfName.ToLower = "wipeout" Then wipeouts.Add(id))
                If wipeouts.Count = 0 Then Continue For

                Dim dot = tr.GetDrawOrderTable(btr.DrawOrderTableId, OpenMode.ForWrite)
                dot.MoveToBottom(wipeouts)
                referenceIds.AddRange(btr.GetBlockReferenceIds(False, False).ToArray)
            Next
            For Each referenceId In referenceIds
                Dim reference = tr.GetBlockReference(referenceId, OpenMode.ForWrite)
                If NotNothing(reference) Then reference.Visible = reference.Visible
            Next
            tr.Commit()
        End Using
    End Sub

End Class
