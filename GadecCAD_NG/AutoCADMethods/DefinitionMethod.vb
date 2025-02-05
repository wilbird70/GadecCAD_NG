'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports GadecCAD_NG.Extensions

''' <summary>
''' Provides methods for blockreferences.
''' </summary>
Public Class DefinitionMethod

    ''' <summary>
    ''' Exports all blockdefinitions in the document to separate dwg-files that are stored in a folder of appdata.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    Public Shared Sub ExportAll(document As Document)
        Dim db = document.Database
        Dim ed = document.Editor

        Dim initialFolder = "{AppDataFolder}\ExportBlockDefinitions".Compose
        FileSystemHelper.CreateFolder(initialFolder)

        Dim folderName = FileSystemHelper.SelectFolder(initialFolder)
        If folderName = "" Then Exit Sub

        FileSystemHelper.CreateFolder(folderName)
        FileSystemHelper.DeleteAllFiles(folderName)
        Using sysVar = New SysVarHandler(document)
            sysVar.Set("FILEDIA", 0)
            sysVar.Set("ATTDIA", 0)
            sysVar.Set("ATTREQ", 0)
            Using tr = db.TransactionManager.StartTransaction()
                Dim bt = tr.GetBlockTable(db.BlockTableId, openErased:=False)
                For Each objectId In bt
                    Dim btr = tr.GetBlockTableRecord(objectId, openErased:=False)
                    If btr.IsAnonymous Or btr.IsLayout Then btr.Dispose() : Continue For

                    ed.Command("_WBLOCK", "{0}\{1}".Compose(folderName, btr.Name), btr.Name)
                    btr.Dispose()
                Next
            End Using
        End Using
        Process.Start(folderName)
    End Sub

End Class
