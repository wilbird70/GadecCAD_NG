'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.Colors
Imports Autodesk.AutoCAD.DatabaseServices
Imports GadecCAD_NG.Extensions

''' <summary>
''' <para><see cref="LayerHandler"/> can create, adjust or select a layer.</para>
''' </summary>
Public Class LayerHandler
    Private ReadOnly _database As Database

    ''' <summary>
    ''' Initializes a new instance of <see cref="LayerHandler"/> for the specified drawing.
    ''' <para><see cref="LayerHandler"/> can create, adjust or select a layer.</para>
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    Public Sub New(database As Database)
        _database = database
    End Sub

    ''' <summary>
    ''' Creates (or adjusts) a layer with the specified name and colorindex.
    ''' </summary>
    ''' <param name="layerName">The name of the layer.</param>
    ''' <param name="colorIndex">The AutoCAD colorindex for the new (or adjusted) layer.</param>
    ''' <param name="visible">Makes the layer invisible if set to false.</param>
    ''' <returns>The objectid of the layer.</returns>
    Public Function Create(layerName As String, colorIndex As Integer, Optional visible As Boolean = True) As ObjectId
        If IsNothing(_database) Then Return ObjectId.Null

        Dim layerId As ObjectId
        Using tr = _database.TransactionManager.StartTransaction
            Dim layerTable = tr.GetLayerTable(_database.LayerTableId, OpenMode.ForWrite)
            Dim layerTableRecord As LayerTableRecord
            Select Case layerTable.Has(layerName)
                Case True : layerTableRecord = tr.GetLayerTableRecord(layerTable(layerName), OpenMode.ForWrite)
                Case Else : layerTableRecord = New LayerTableRecord With {.LineWeight = LineWeight.ByLineWeightDefault, .Name = layerName}
                    layerTable.Add(layerTableRecord)
                    tr.AddNewlyCreatedDBObject(layerTableRecord, True)
            End Select
            layerTableRecord.Color = Color.FromColorIndex(ColorMethod.ByAci, colorIndex)
            layerTableRecord.IsOff = Not visible
            layerId = layerTableRecord.ObjectId
            tr.Commit()
        End Using
        Return layerId
    End Function

    ''' <summary>
    ''' Selects the layer specified by its objectid to be current.
    ''' </summary>
    ''' <param name="layerId">The objectid of a layer.</param>
    ''' <returns>The layername.</returns>
    Public Function SelectById(layerId As ObjectId) As String
        If IsNothing(_database) Then Return ""

        Dim layerName = ""
        Using tr = _database.TransactionManager.StartTransaction
            Dim layerTableRecord = tr.GetLayerTableRecord(layerId)
            If IsNothing(layerTableRecord) Then Return ""

            layerName = layerTableRecord.Name
            If Not layerId = _database.Clayer Then
                If layerTableRecord.IsOff Then layerTableRecord.IsOff = False
                If layerTableRecord.IsFrozen Then layerTableRecord.IsFrozen = False
                If layerTableRecord.IsLocked Then layerTableRecord.IsLocked = False
                _database.Clayer = layerId
            End If
            tr.Commit()
        End Using
        Return layerName
    End Function

End Class
