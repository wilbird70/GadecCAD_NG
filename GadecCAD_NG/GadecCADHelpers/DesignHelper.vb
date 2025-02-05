'Gadec Engineerings Software (c) 2022
Imports GadecCAD_NG.Extensions

Public Class DesignHelper

    'functions

    ''' <summary>
    ''' Gets the symboldescriptions of all DesignCenter files.
    ''' </summary>
    ''' <param name="language">The language to use.</param>
    ''' <returns>A dictionary with descriptions.</returns>
    Public Shared Function GetSymbolDescriptions(Optional language As String = "") As Dictionary(Of String, String)
        Dim moduleData = DataSetHelper.LoadFromXml("{Support}\SetDesignCenter.xml".Compose).GetTable("Modules", "Name")
        If IsNothing(moduleData) Then Return New Dictionary(Of String, String)

        Dim output = New Dictionary(Of String, String)
        For Each moduleRow In moduleData.Select
            Dim symbolData = DataSetHelper.LoadFromXml("{Support}\{0}".Compose(moduleRow.GetString("File"))).GetTable("Blocks")
            If IsNothing(symbolData) Then Continue For

            For Each symbolRow In symbolData.Select
                Dim blockName = symbolRow.GetString("BlockName")
                If Not blockName.StartsWith("_") Then Continue For

                Select Case language = ""
                    Case True : output.TryAdd(blockName.EraseStart(1), symbolRow.GetTranslation)
                    Case Else : output.TryAdd(blockName.EraseStart(1), symbolRow.GetString(language))
                End Select
            Next
        Next
        Return output
    End Function

    ''' <summary>
    ''' Gets the data from all DesignCenter files in one database.
    ''' </summary>
    ''' <returns>The database.</returns>
    Public Shared Function GetAllDesignCenterData() As Data.DataTable
        Dim moduleData = DataSetHelper.LoadFromXml("{Support}\SetDesignCenter.xml".Compose).GetTable("Modules", "Name")
        If IsNothing(moduleData) Then Return Nothing

        Dim symbolData = New Data.DataTable("Blocks")
        Dim pageData = New Data.DataTable("Pages")
        Dim indexField = New Data.DataColumn("Index", GetType(Int32))
        Dim moduleField1 = New Data.DataColumn("Module", GetType(String))
        Dim moduleField2 = New Data.DataColumn("Module", GetType(String))
        Dim countField = New Data.DataColumn("Count", GetType(String))
        indexField.AutoIncrement = True
        indexField.AutoIncrementSeed = 1
        indexField.AutoIncrementStep = 1
        symbolData.Columns.Add(indexField)
        symbolData.Columns.Add(moduleField1)
        pageData.Columns.Add(moduleField2)
        symbolData.Columns.Add(countField)
        For Each moduleRow In moduleData.Rows.ToArray
            Dim fileName = moduleRow.GetString("File")
            Dim blocksData = DataSetHelper.LoadFromXml("{Support}\{0}".Compose(fileName)).GetTable("Blocks")
            If IsNothing(blocksData) Then Continue For

            moduleField1.DefaultValue = fileName
            moduleField2.DefaultValue = fileName
            symbolData.Merge(blocksData)
            pageData.Merge(blocksData.GetTable("Pages"))
        Next
        pageData.AssignPrimaryKey("Module;Name")
        Dim dataSet = New Data.DataSet
        dataSet.Tables.Add(symbolData)
        dataSet.Tables.Add(pageData)
        Return symbolData
    End Function

End Class
