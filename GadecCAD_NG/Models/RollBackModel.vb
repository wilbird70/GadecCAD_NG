'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.Geometry

''' <summary>
''' <para><see cref="RollBackModel"/> contains data to roll back the symbol encoding one step.</para>
''' </summary>
Public Class RollBackModel
    ''' <summary>
    ''' The attribute texts to roll back.
    ''' </summary>
    ''' <returns>A dictionary containing the objectids of the attribute references and their texts.</returns>
    Public ReadOnly Property Strings As Dictionary(Of ObjectId, String)
    ''' <summary>
    ''' The objectid of the line drawn, if any.
    ''' </summary>
    ''' <returns>The objectid.</returns>
    Public ReadOnly Property ObjectId As ObjectId?
    ''' <summary>
    ''' The startpoint of this step.
    ''' </summary>
    ''' <returns>The point3d.</returns>
    Public ReadOnly Property Point3d As Point3d?
    ''' <summary>
    ''' The number of symbols to roll back.
    ''' </summary>
    ''' <returns>The number.</returns>
    Public ReadOnly Property Number As Integer

    ''' <summary>
    ''' Initializes a new instance of <see cref="RollBackModel"/> with the specified properties.
    ''' <para><see cref="RollBackModel"/> contains data to roll back the symbol encoding one step.</para>
    ''' </summary>
    ''' <param name="strings">The attribute texts to roll back.</param>
    ''' <param name="number">The number of symbols to roll back.</param>
    ''' <param name="objectId">The objectid of the line drawn, if any.</param>
    ''' <param name="point3d">The startpoint of this step.</param>
    Public Sub New(strings As Dictionary(Of ObjectId, String), Optional objectId As ObjectId = Nothing, Optional point3d As Point3d = Nothing, Optional number As Integer = 0)
        _Strings = strings
        _Number = number
        _ObjectId = objectId
        _Point3d = point3d
    End Sub

    ''' <summary>
    ''' Initializes a new instance of <see cref="RollBackModel"/> with the specified properties.
    ''' <para><see cref="RollBackModel"/> contains data to roll back the symbol encoding one step.</para>
    ''' </summary>
    ''' <param name="objectId">The objectid of the line drawn, if any.</param>
    ''' <param name="point3d">The startpoint of this step.</param>
    Public Sub New(objectId As ObjectId, Optional point3d As Point3d = Nothing)
        _Strings = Nothing
        _Number = Nothing
        _ObjectId = objectId
        _Point3d = point3d
    End Sub

End Class
