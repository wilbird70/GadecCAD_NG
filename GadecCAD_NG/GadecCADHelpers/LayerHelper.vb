'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.DatabaseServices
Imports GadecCAD_NG.Extensions

''' <summary>
''' Provides methods for discipline-typed layers.
''' <para>Discipline-typed layers are defined in the Layers-database of the SetStandards.xml-file.</para>
''' </summary>
Public Class LayerHelper

    ''' <summary>
    ''' Gets the index-number of the layer-type of the current layer.
    ''' <para>Note: If current layer is not a type of the current discipline, it returns -1.</para>
    ''' </summary>
    ''' <returns>The index-number.</returns>
    Public Shared Function GetCurrentLayerTypeIndex() As Integer
        Dim layerData = DataSetHelper.LoadFromXml("{Support}\SetStandards.xml".Compose).GetTable("Layers", "LName")
        Dim currentLayer As String = SysVarHandler.GetVar("CLAYER")
        Dim layerRow = layerData.Rows.Find(currentLayer)
        If IsNothing(layerRow) Then Return -1

        Select Case layerRow.GetString("Name") = Registerizer.UserSetting("Discipline")
            Case True : Return layerRow.GetString("Index").ToInteger
            Case Else : Return -1
        End Select
    End Function

    ''' <summary>
    ''' Gets the objectid of a discipline-typed layer.
    ''' <para>Note: If layertype is not a type of the current discipline, it returns the objectid of the current layer.</para>
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="layerType">Discipline-typed layer name.</param>
    ''' <returns>The objectid.</returns>
    Public Shared Function GetLayerIdFromType(database As Database, layerType As String) As ObjectId
        Dim layerData = DataSetHelper.LoadFromXml("{Support}\SetStandards.xml".Compose).GetTable("Layers", "Name;Type")
        Dim layerRow = layerData.Rows.Find({Registerizer.UserSetting("Discipline"), layerType})
        If IsNothing(layerRow) Then Return database.Clayer

        Dim layerName = layerRow.GetString("LName")
        Dim layerColor = layerRow.GetString("Color").ToInteger
        Dim LayerHandler = New LayerHandler(database)
        Return LayerHandler.Create(layerName, layerColor, True)
    End Function

End Class
