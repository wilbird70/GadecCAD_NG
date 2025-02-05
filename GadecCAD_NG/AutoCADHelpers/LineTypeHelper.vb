'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.DatabaseServices
Imports GadecCAD_NG.Extensions

''' <summary>
''' Provides methods for linetypes.
''' </summary>
Public Class LineTypeHelper

    ''' <summary>
    ''' Gets the objectid of the specified linetype.
    ''' <para>If the linetype is not available in the drawing, it tries to load it from the acadiso.lin-file.</para>
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="lineType">The name of the linetype.</param>
    ''' <returns>The objectid.</returns>
    Public Shared Function GetLineTypeId(database As Database, lineType As String) As ObjectId
        Dim output As ObjectId
        Using tr = database.TransactionManager.StartTransaction
            Dim linetypeTable = tr.GetLinetypeTable(database.LinetypeTableId)
            If Not linetypeTable.Has(lineType) Then database.LoadLineTypeFile(lineType, "acadiso.lin")
            Select Case linetypeTable.Has(lineType)
                Case True : output = linetypeTable(lineType)
                Case Else : output = database.Celtype
            End Select
            tr.Commit()
        End Using
        Return output
    End Function

End Class
