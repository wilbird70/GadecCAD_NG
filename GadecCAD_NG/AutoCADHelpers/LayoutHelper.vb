'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports GadecCAD_NG.Extensions

''' <summary>
''' Provides methods for getting and selecting layouts.
''' </summary>
Public Class LayoutHelper

    ''' <summary>
    ''' Selects the model layout to be current.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <returns>The layoutname.</returns>
    Public Shared Function SelectModel(document As Document) As String
        Using document.LockDocument
            Dim layoutManager = Autodesk.AutoCAD.DatabaseServices.LayoutManager.Current
            layoutManager.CurrentLayout = "Model"
            Return layoutManager.CurrentLayout
        End Using
    End Function

    ''' <summary>
    ''' Selects a layout specified by its objectid.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="layoutId">The objectid of the layout.</param>
    ''' <returns></returns>
    Public Shared Function SelectById(document As Document, layoutId As ObjectId) As String
        Using document.LockDocument
            Return SelectById(document.Database, layoutId)
        End Using
    End Function

    ''' <summary>
    ''' Selects a layout specified by its objectid.
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="layoutId">The objectid of the layout.</param>
    ''' <returns></returns>
    Public Shared Function SelectById(database As Database, layoutId As ObjectId) As String
        Dim layout = GetLayoutById(database, layoutId)
        If IsNothing(layout) Then Return ""

        Dim output = layout.LayoutName
        Dim layoutManager = Autodesk.AutoCAD.DatabaseServices.LayoutManager.Current
        If Not layoutManager.CurrentLayout = output Then layoutManager.CurrentLayout = output
        Return output
    End Function

    ''' <summary>
    ''' Gets the names of all layouts in the current drawing.
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <returns>A list of layoutnames.</returns>
    Public Shared Function GetNames(database As Database) As String()
        Using tr = database.TransactionManager.StartTransaction()
            Dim layouts = tr.GetDBDictionary(database.LayoutDictionaryId)
            Return layouts.ToList.Select(Function(layout) layout.Key)
            tr.Commit()
        End Using
    End Function

    ''' <summary>
    ''' Gets the objectid of the current layout.
    ''' </summary>
    ''' <returns>The objectid.</returns>
    Public Shared Function GetCurrentId() As ObjectId
        Dim layoutManager = Autodesk.AutoCAD.DatabaseServices.LayoutManager.Current
        Return layoutManager.GetLayoutId(layoutManager.CurrentLayout)
    End Function

    ''' <summary>
    ''' Gets the name of the current layout.
    ''' </summary>
    ''' <returns>The layoutname.</returns>
    Public Shared Function GetCurrentName() As String
        Dim layoutManager = Autodesk.AutoCAD.DatabaseServices.LayoutManager.Current
        Return layoutManager.CurrentLayout
    End Function

    ''' <summary>
    ''' Gets the name of a layout specified by its objectid.
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="layoutId">The objectid of the layout.</param>
    ''' <returns></returns>
    Public Shared Function GetNameById(database As Database, layoutId As ObjectId) As String
        Dim layout = GetLayoutById(database, layoutId)
        If IsNothing(layout) Then Return ""

        Return layout.LayoutName
    End Function

    ''' <summary>
    ''' Gets a layout specified by its objectid.
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="layoutId">The objectid of the layout.</param>
    ''' <returns>The layout.</returns>
    Public Shared Function GetLayoutById(database As Database, layoutId As ObjectId) As Layout
        Using tr = database.TransactionManager.StartTransaction
            Return tr.GetLayout(layoutId)
            tr.Commit()
        End Using
    End Function

    ''' <summary>
    ''' Gets the objectid of a layout specified by its name.
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="layoutName">The name of the layout.</param>
    ''' <returns>The objectid.</returns>
    Public Shared Function GetIdByName(database As Database, layoutName As String) As ObjectId
        Dim output = ObjectId.Null
        Using tr = database.TransactionManager.StartTransaction
            Dim layouts = tr.GetDBDictionary(database.LayoutDictionaryId)
            If layouts.Contains(layoutName) Then output = layouts(layoutName)
            tr.Commit()
        End Using
        Return output
    End Function

End Class
