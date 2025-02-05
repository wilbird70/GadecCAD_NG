'Gadec Engineerings Software (c) 2022
Imports System.Data
Imports System.Runtime.CompilerServices
Imports Autodesk.AutoCAD.DatabaseServices

Namespace Extensions

    Public Module DataTableExtensions

        ''' <summary>
        ''' Gets all the fieldnames that stores attribute-reference information.
        ''' </summary>
        ''' <param name="eDataTable"></param>
        ''' <returns>List of fieldnames.</returns>
        <Extension()>
        Public Function GetAttributeColumns(ByVal eDataTable As Data.DataTable) As String()
            Dim output = eDataTable.GetColumnNames.ToList.FindAll(Function(x) x.EndsWith("_ID"))
            For i = 0 To output.Count - 1
                output(i) = output(i).EraseEnd(3)
            Next
            Return output.ToArray
        End Function

        ''' <summary>
        ''' Gets the ObjectIds in the specified field of all records.
        ''' </summary>
        ''' <param name="eDataTable"></param>
        ''' <param name="columnName">The name of the field.</param>
        ''' <returns>The list of ObjectIds.</returns>
        <Extension()>
        Public Function GetObjectIDsFromColumn(ByVal eDataTable As Data.DataTable, columnName As String) As ObjectId()
            Select Case True
                Case Not eDataTable.Columns.Contains(columnName) : Return {}
                Case Not eDataTable.Columns(columnName).DataType = GetType(ObjectId) : Return {}
                Case Else : Return eDataTable.AsEnumerable().Select(Function(x) x(columnName)).Cast(Of ObjectId)().ToArray
            End Select
        End Function

    End Module

End Namespace