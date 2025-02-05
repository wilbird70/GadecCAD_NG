'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports GadecCAD_NG.Extensions

''' <summary>
''' <para><see cref="LayerVisiblizer"/> thaws and turns on all layers, but keeps the current settings to reset them.</para>
''' <para>Note: This class is used to plot drawings without opening them in the drawing viewer.</para>
''' <para>It seems that AutoCAD uses the frozen and off settings of the active document's layer table instead of the layer table of the document to be plotted.</para>
''' </summary>
Public Class LayerVisiblizer
    Implements IDisposable
    Private _disposed As Boolean

    ''' <summary>
    ''' Contains the present document.
    ''' </summary>
    Private ReadOnly _document As Document
    ''' <summary>
    ''' Contains the previous layerstates.
    ''' </summary>
    Private _layerStates As New Dictionary(Of ObjectId, (Frozen As Boolean, Off As Boolean))

    ''' <summary>
    ''' Initializes a new instance of <see cref="LayerVisiblizer"/> for the present document.
    ''' <para><see cref="LayerVisiblizer"/> thaws and turns on all layers, but keeps the current settings to reset them.</para>
    ''' <para>Note: This class is used to plot drawings without opening them in the drawing viewer.</para>
    ''' <para>It seems that AutoCAD uses the frozen and off settings of the active document's layer table instead of the layer table of the document to be plotted.</para>
    ''' </summary>
    ''' <param name="document">The present document.</param>
    Public Sub New(document As Document)
        _document = document
        Dim db = document.Database
        Using document.LockDocument
            Using tr = db.TransactionManager.StartTransaction()
                Dim layerTable = tr.GetLayerTable(db.LayerTableId)
                For Each objectId In layerTable
                    Dim layerTableRecord = tr.GetLayerTableRecord(objectId, OpenMode.ForWrite)
                    If layerTableRecord.IsFrozen Or layerTableRecord.IsOff Then
                        _layerStates.Add(objectId, (layerTableRecord.IsFrozen, layerTableRecord.IsOff))
                        layerTableRecord.IsFrozen = False
                        layerTableRecord.IsOff = False
                    End If
                Next
                tr.Commit()
            End Using
        End Using
    End Sub

    ''' <summary>
    ''' Reset the layer states to previous settings.
    ''' </summary>
    ''' <param name="disposing"></param>
    Protected Overridable Sub Dispose(disposing As Boolean)
        If _disposed Then Exit Sub

        If disposing Then Reset()
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

    'private subs

    ''' <summary>
    ''' Reset the layer states to previous settings.
    ''' </summary>
    Private Sub Reset()
        Dim db = _document.Database
        Using _document.LockDocument
            Using tr = db.TransactionManager.StartTransaction()
                Dim layerTable = tr.GetLayerTable(db.LayerTableId)
                For Each objectId In layerTable
                    Dim layerTableRecord = tr.GetLayerTableRecord(objectId, OpenMode.ForWrite)
                    If _layerStates.ContainsKey(objectId) Then
                        Dim state = _layerStates(objectId)
                        If state.Frozen Then layerTableRecord.IsFrozen = True
                        If state.Off Then layerTableRecord.IsOff = True
                    End If
                Next
                tr.Commit()
            End Using
        End Using
        _layerStates = Nothing
    End Sub

End Class
