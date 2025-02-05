Imports Autodesk.AutoCAD.DatabaseServices

''' <summary>
''' Provides methods to transpose a canonical media name to the locale media name.
''' </summary>
Public Class PlotMediaHelper
    Private Shared ReadOnly _plotMediaFile As String = "{AppDataFolder}\LocaleMediaData.xml".Compose

    'functions

    ''' <summary>
    ''' Gets the locale media name from the previously saved data.
    ''' </summary>
    ''' <param name="deviceName">The name of the plotter/printer device.</param>
    ''' <param name="paperName">The canonical media name.</param>
    ''' <returns>The locale media name.</returns>
    Public Shared Function GetLocaleMediaName(deviceName As String, paperName As String) As String
        FileSystemHelper.CreateFolder(IO.Path.GetDirectoryName(_plotMediaFile))
        Dim mediaDataSet = DataSetHelper.LoadFromXml(_plotMediaFile)
        If Not mediaDataSet.Tables.Contains(deviceName) Then Return ""

        Dim mediaData = mediaDataSet.GetTable(deviceName, "PaperName")
        Dim mediaRow = mediaData.Rows.Find(paperName)
        If IsNothing(mediaRow) Then Return ""

        Return mediaRow.GetString("MediaName")
    End Function

    ''' <summary>
    ''' Gets the locale media name from the specified plotsettings.
    ''' <para>Note: It saves all locale media names for this device for future reference.</para>
    ''' </summary>
    ''' <param name="plotSettings">The present PlotSettings.</param>
    ''' <param name="deviceName">The name of the plotter/printer device.</param>
    ''' <param name="paperName">The canonical media name.</param>
    ''' <returns>The locale media name.</returns>
    Public Shared Function GetLocaleMediaName(plotSettings As PlotSettings, deviceName As String, paperName As String) As String
        FileSystemHelper.CreateFolder(IO.Path.GetDirectoryName(_plotMediaFile))
        Dim mediaDataSet = DataSetHelper.LoadFromXml(_plotMediaFile)
        If mediaDataSet.Tables.Contains(deviceName) Then mediaDataSet.Tables.Remove(deviceName)
        Dim dataBuilder = New DataBuilder(deviceName, "PaperName;MediaName".Cut)
        Try
            Dim plotSettingsValidator = Autodesk.AutoCAD.DatabaseServices.PlotSettingsValidator.Current
            plotSettingsValidator.SetPlotConfigurationName(plotSettings, deviceName, Nothing)
            Dim canonicalMediaNameList = plotSettingsValidator.GetCanonicalMediaNameList(plotSettings)
            For Each mediaName In canonicalMediaNameList
                Dim localeMediaName = plotSettingsValidator.GetLocaleMediaName(plotSettings, mediaName)
                dataBuilder.AppendValue("PaperName", localeMediaName)
                dataBuilder.AppendValue("MediaName", mediaName)
                dataBuilder.AddNewlyCreatedRow()
            Next
        Catch ex As Exception
            Return ""
        End Try
        mediaDataSet.Tables.Add(dataBuilder.GetDataTable)
        mediaDataSet.WriteXml(_plotMediaFile)
        Dim newData = mediaDataSet.GetTable(deviceName, "PaperName")
        If IsNothing(newData) Then MsgBox("Printer not available".Translate(deviceName)) : Return ""

        Dim newRow = newData.Rows.Find(paperName)
        If IsNothing(newRow) Then MsgBox("Papername not available".Translate(paperName, deviceName)) : Return ""

        Return newRow.GetString("MediaName")
    End Function

    'subs

    ''' <summary>
    ''' Resets the previously saved data (locale media names) for the specified device.
    ''' </summary>
    ''' <param name="deviceName">The name of the plotter/printer device.</param>
    Public Shared Sub ResetLocaleMediaData(deviceName As String)
        Dim mediaDataSet = DataSetHelper.LoadFromXml(_plotMediaFile)
        If mediaDataSet.Tables.Contains(deviceName) Then mediaDataSet.Tables.Remove(deviceName)
        mediaDataSet.WriteXml(_plotMediaFile)
    End Sub

End Class
