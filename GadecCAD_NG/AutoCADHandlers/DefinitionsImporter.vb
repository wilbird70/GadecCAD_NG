'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports GadecCAD_NG.Extensions

''' <summary>
''' <para><see cref="DefinitionsImporter"/> can import (nested) blockdefinitions from another dwg-file.</para>
''' </summary>
Public Class DefinitionsImporter
    Implements System.IDisposable
    Private _disposed As Boolean

    ''' <summary>
    ''' The name of the source-file without its path.
    ''' </summary>
    ''' <returns>The filename.</returns>
    Public ReadOnly Property FileName As String

    ''' <summary>
    ''' The drawing database of the sourcefile from which the definitions will be imported.
    ''' </summary>
    Private ReadOnly _sourceDb As Database = Nothing

    'class

    ''' <summary>
    ''' Initializes a new instance of <see cref="DefinitionsImporter"/> with the specified source-file.
    ''' <para><see cref="DefinitionsImporter"/> can import (nested) blockdefinitions from another dwg-file.</para>
    ''' </summary>
    ''' <param name="sourceFile">The fullname of the source-file.</param>
    Public Sub New(sourceFile As String)
        _FileName = IO.Path.GetFileName(sourceFile)
        If sourceFile = "" Then Exit Sub
        If Not IO.File.Exists(sourceFile) Then MessageBoxInfo("FileNotFound".Translate(sourceFile)) : Exit Sub

        _sourceDb = New Database(False, True)
        _sourceDb.ReadDwgFile(sourceFile, FileOpenMode.OpenForReadAndAllShare, True, "")
    End Sub

    ''' <summary>
    ''' Unlocks (disposes) the drawing database of the sourcefile when disposing.
    ''' </summary>
    Protected Overridable Sub Dispose(disposing As Boolean)
        If _disposed Then Exit Sub

        If disposing Then
            If NotNothing(_sourceDb) Then
                DirectCast(_sourceDb, IDisposable).Dispose()
            End If
        End If
        _disposed = True
    End Sub

    ''' <summary>
    ''' Implements the dispose method.
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
        Dispose(disposing:=True)
        GC.SuppressFinalize(Me)
    End Sub

    'functions

    ''' <summary>
    ''' Imports or redefines the blockdefinition from the sourcefile with the specified blockname.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="blockName">The name of the blockdefinition.</param>
    ''' <returns>The objectid of the blockdefinition.</returns>
    Public Function ImportDefinition(document As Document, blockName As String) As ObjectId
        If IsNothing(_sourceDb) Then Return ObjectId.Null

        Using document.LockDocument
            Return ImportDefinition(document.Database, blockName)
        End Using
    End Function

    ''' <summary>
    ''' Imports or redefines the blockdefinition from the sourcefile with the specified blockname.
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="blockName">The name of the blockdefinition.</param>
    ''' <returns>The objectid of the blockdefinition.</returns>
    Public Function ImportDefinition(database As Database, blockName As String) As ObjectId
        If IsNothing(_sourceDb) Then Return ObjectId.Null

        Try
            Dim sourceId As ObjectId
            Using sourceTr = _sourceDb.TransactionManager.StartTransaction()
                Dim bt = sourceTr.GetBlockTable(_sourceDb.BlockTableId, openErased:=False)
                If Not bt.Has(blockName) Then MessageBoxInfo("BlockNotFound".Translate(blockName, IO.Path.GetFileName(_sourceDb.Filename))) : Return ObjectId.Null

                sourceId = bt.Item(blockName)
                sourceTr.Commit()
            End Using
            Dim idMapping = New IdMapping
            _sourceDb.WblockCloneObjects(New ObjectIdCollection({sourceId}), database.BlockTableId, idMapping, DuplicateRecordCloning.Replace, False)
            TransferGroups(database, idMapping)
            TransferRelativeDrawOrder(database, idMapping)
            Return idMapping.Lookup(sourceId).Value
        Catch ex As Autodesk.AutoCAD.Runtime.Exception
            ex.AddData($"source: {_sourceDb.Filename}")
            ex.AddData($"block: {blockName}")
            ex.Rethrow
            Return ObjectId.Null
        End Try
    End Function

    ''' <summary>
    ''' Imports or redefines the blockdefinitions contained (nested) in the blockdefinition from the sourcefile with the specified block name.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="blockName">The name of the blockdefinition.</param>
    ''' <returns>The names of the blockdefinitions.</returns>
    Public Function ImportNestedDefinitions(document As Document, blockName As String) As String()
        If IsNothing(_sourceDb) Then Return {}

        Try
            Dim output = New List(Of String)
            Dim sourceIds = New ObjectIdCollection()
            Using tr = _sourceDb.TransactionManager.StartTransaction()
                Dim bt = tr.GetBlockTable(_sourceDb.BlockTableId, openErased:=False)
                If bt.Has(blockName) Then
                    Dim sourceDefinition = tr.GetBlockTableRecord(bt.Item(blockName))
                    For Each entityId In sourceDefinition
                        Dim reference = tr.GetBlockReference(entityId)
                        If IsNothing(reference) Then Continue For

                        Dim definitionId = If(reference.IsDynamicBlock, reference.DynamicBlockTableRecord, reference.BlockTableRecord)
                        output.Add(tr.GetBlockTableRecord(definitionId).Name)
                        sourceIds.Add(definitionId)
                    Next
                End If
            End Using
            Using document.LockDocument
                _sourceDb.WblockCloneObjects(sourceIds, document.Database.BlockTableId, New IdMapping, DuplicateRecordCloning.Replace, False)
            End Using
            Return output.ToArray
        Catch ex As System.Exception
            ex.AddData($"source: {_sourceDb.Filename}")
            ex.AddData($"block: {blockName}")
            ex.Rethrow
            Return {}
        End Try
    End Function

    'private subs

    ''' <summary>
    ''' Transfers the existing groups in the original blockdefinition to the imported ones in the current drawing.
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="idMapping">Mapping between the objectids of the original and the cloned objects.</param>
    Private Sub TransferGroups(database As Database, idMapping As IdMapping)
        Using tr = database.TransactionManager.StartTransaction()
            Using sourceTr = _sourceDb.TransactionManager.StartTransaction()
                Dim groupedObjectIds = New List(Of ObjectId)
                For Each pair In idMapping.Cast(Of IdPair)
                    If groupedObjectIds.Contains(pair.Key) Then Continue For

                    Dim sourceDbObject = sourceTr.GetObject(pair.Key)
                    If IsNothing(sourceDbObject) Then Continue For

                    For Each persistentReactorId In sourceDbObject.GetPersistentReactorIds.ToArray
                        Dim sourceGroup = sourceTr.GetGroup(persistentReactorId)
                        If IsNothing(sourceGroup) Then Continue For

                        Dim sourceGroupEntityIds = sourceGroup.GetAllEntityIds
                        Dim targetGroupEntityIds = New ObjectIdCollection
                        For Each objectId In sourceGroupEntityIds
                            If idMapping.Contains(objectId) Then targetGroupEntityIds.Add(idMapping.Lookup(objectId).Value)
                        Next
                        GroupHelper.Create(database, targetGroupEntityIds)
                        groupedObjectIds.AddRange(sourceGroupEntityIds)
                    Next
                Next
                sourceTr.Commit()
            End Using
            tr.Commit()
        End Using
    End Sub

    ''' <summary>
    ''' Transfers the existing draworder in the original blockdefinition to the imported ones in the current drawing.
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="idMapping">Mapping between the objectids of the original and the cloned objects.</param>
    Private Sub TransferRelativeDrawOrder(database As Database, idMapping As IdMapping)
        Using tr = database.TransactionManager.StartTransaction()
            Using sourceTr = _sourceDb.TransactionManager.StartTransaction()
                For Each pair In idMapping.Cast(Of IdPair)
                    Dim sourceBtr = sourceTr.GetBlockTableRecord(pair.Key)
                    If IsNothing(sourceBtr) Then Continue For

                    Dim sourceDrawOrder = sourceTr.GetDrawOrderTable(sourceBtr.DrawOrderTableId).GetFullDrawOrder(0)
                    Dim targetDrawOrder = New ObjectIdCollection()
                    For Each objectId In sourceDrawOrder.ToArray
                        If idMapping.Contains(objectId) Then targetDrawOrder.Add(idMapping(objectId).Value)
                    Next
                    Try
                        Dim targetBtr = tr.GetBlockTableRecord(pair.Value)
                        Dim targetDrawOrderTable = tr.GetDrawOrderTable(targetBtr.DrawOrderTableId, OpenMode.ForWrite)
                        targetDrawOrderTable.SetRelativeDrawOrder(targetDrawOrder)
                    Catch ex As Exception
                        'If setting the relative draworder throws an exception, do nothing (not so important).
                    End Try
                Next
                sourceTr.Commit()
            End Using
            tr.Commit()
        End Using
    End Sub

End Class
