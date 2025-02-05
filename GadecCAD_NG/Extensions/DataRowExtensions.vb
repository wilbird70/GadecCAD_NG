'Gadec Engineerings Software (c) 2022
Imports System.Data
Imports System.Runtime.CompilerServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.Geometry

Namespace Extensions

    Public Module DataRowExtensions

        ''' <summary>
        ''' Gets all the fieldnames that stores attribute-reference information.
        ''' </summary>
        ''' <param name="eDataRow"></param>
        ''' <returns>List of fieldnames.</returns>
        <Extension()>
        Public Function GetAttributeColumns(ByVal eDataRow As DataRow) As String()
            Return eDataRow.Table.GetAttributeColumns
        End Function

        ''' <summary>
        ''' Determine if the field stores attribute-reference information.
        ''' </summary>
        ''' <param name="eDataRow"></param>
        ''' <param name="columnName">The name of the field.</param>
        ''' <returns></returns>
        <Extension()>
        Public Function HasAttribute(ByVal eDataRow As DataRow, columnName As String) As Boolean
            Dim attributeIdColumn = columnName & "_ID"
            Select Case True
                Case Not eDataRow.Table.Columns.Contains(columnName) : Return False
                Case Not eDataRow.Table.Columns.Contains(attributeIdColumn) : Return False
                Case IsDBNull(eDataRow(columnName)) : Return False
                Case IsDBNull(eDataRow(attributeIdColumn)) : Return False
                Case Else : Return True
            End Select
        End Function

        ''' <summary>
        ''' Gets the <see cref="ObjectId"/> of the attribute-references stored in the specified field.
        ''' </summary>
        ''' <param name="eDataRow"></param>
        ''' <param name="columnName">The name of the field.</param>
        ''' <returns>The <see cref="ObjectId"/></returns>
        <Extension()>
        Public Function GetAttributeId(ByVal eDataRow As DataRow, columnName As String) As ObjectId
            Dim attributeIdColumn = columnName & "_ID"
            Select Case True
                Case Not eDataRow.Table.Columns.Contains(attributeIdColumn) : Return Nothing
                Case IsDBNull(eDataRow(attributeIdColumn)) : Return Nothing
                Case Else : Return eDataRow(attributeIdColumn)
            End Select
        End Function

        ''' <summary>
        ''' Gets the <see cref="Point3d"/> stored the specified field.
        ''' </summary>
        ''' <param name="eDataRow"></param>
        ''' <param name="columnName">The name of the field.</param>
        ''' <returns>The <see cref="Point3d"/></returns>
        <Extension()>
        Public Function GetPoint3d(ByVal eDataRow As DataRow, columnName As String) As Point3d
            Return DirectCast(eDataRow.GetValue(columnName), Point3d)
        End Function

        ''' <summary>
        ''' Gets the <see cref="Extents3d"/> stored the specified field.
        ''' </summary>
        ''' <param name="eDataRow"></param>
        ''' <param name="columnName">The name of the field.</param>
        ''' <returns>The <see cref="Extents3d"/></returns>
        <Extension()>
        Public Function GetExtents3d(ByVal eDataRow As DataRow, columnName As String) As Extents3d
            Return DirectCast(eDataRow.GetValue(columnName), Extents3d)
        End Function

        ''' <summary>
        ''' Gets the <see cref="ObjectId"/> stored the specified field.
        ''' </summary>
        ''' <param name="eDataRow"></param>
        ''' <param name="columnName">The name of the field.</param>
        ''' <returns>The <see cref="ObjectId"/></returns>
        <Extension()>
        Public Function GetObjectId(ByVal eDataRow As DataRow, columnName As String) As ObjectId
            If Not eDataRow.HasValue(columnName) Then Return ObjectId.Null

            Return DirectCast(eDataRow.GetValue(columnName), ObjectId)
        End Function

        ''' <summary>
        ''' Gets the <see cref="ObjectIdCollection"/> stored the specified field.
        ''' </summary>
        ''' <param name="eDataRow"></param>
        ''' <param name="columnName">The name of the field.</param>
        ''' <returns>The <see cref="ObjectIdCollection"/></returns>
        <Extension()>
        Public Function GetObjectIdCollection(ByVal eDataRow As DataRow, columnName As String) As ObjectIdCollection
            Return DirectCast(eDataRow.GetValue(columnName), ObjectIdCollection)
        End Function

    End Module

End Namespace
