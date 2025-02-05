'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.DatabaseServices
Imports GadecCAD_NG.Extensions

''' <summary>
''' Provides methods for manipulating the visibility and behavior of attribute references.
''' </summary>
Public Class AttributeHelper

    ''' <summary>
    ''' Sets the visibility of a attribute reference. 
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="attributeId">The objectid of the attribute reference.</param>
    ''' <param name="visible">True for visible, false for hidden.</param>
    Public Shared Sub SetVisibility(database As Database, attributeId As ObjectId, visible As Boolean)
        SetVisibility(database, New ObjectIdCollection({attributeId}), visible)
    End Sub

    ''' <summary>
    ''' Sets the visibility of attribute references. 
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="attributeIds">The objectids of the attribute references.</param>
    ''' <param name="visible">True for visible, false for hidden.</param>
    Public Shared Sub SetVisibility(database As Database, attributeIds As ObjectIdCollection, visible As Boolean)
        Using tr = database.TransactionManager.StartTransaction
            For Each attributeId In attributeIds.ToArray
                Dim attribute = tr.GetAttributeReference(attributeId, OpenMode.ForWrite)
                attribute.Invisible = Not visible
            Next
            tr.Commit()
        End Using
    End Sub

End Class
