'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.DatabaseServices
Imports GadecCAD_NG.Extensions

''' <summary>
''' Provides methods for grouping (or groups of) entities.
''' </summary>
Public Class GroupHelper

    'subs

    ''' <summary>
    ''' Creates a new group containing the specified (by objectids) entities.
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="entityIds">A list of objectids.</param>
    Public Shared Sub Create(database As Database, entityIds As ObjectIdCollection)
        Using tr = database.TransactionManager.StartTransaction()
            Dim groupDictionary = tr.GetDBDictionary(database.GroupDictionaryId, OpenMode.ForWrite)
            Dim group = New Group()
            groupDictionary.SetAt(Randomizer.GetString(6), group)
            group.Append(entityIds)
            tr.AddNewlyCreatedDBObject(group, True)
            tr.Commit()
        End Using
    End Sub

    'functions

    ''' <summary>
    ''' Gets the objectids of entities that are in the same group as the specified (by objectid) entity.
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="entityId">The objectid of an entity.</param>
    ''' <returns></returns>
    Public Shared Function GetGroupedEntityIds(database As Database, entityId As ObjectId) As ObjectIdCollection
        Dim output = New ObjectIdCollection
        Using tr = database.TransactionManager.StartTransaction
            Dim entity = tr.GetEntity(entityId)
            For Each objectId In entity.GetPersistentReactorIds.ToArray
                Dim group = tr.GetGroup(objectId)
                If IsNothing(group) Then Continue For

                output = New ObjectIdCollection(group.GetAllEntityIds)
                Exit For
            Next
            tr.Commit()
        End Using
        Return output
    End Function

End Class
