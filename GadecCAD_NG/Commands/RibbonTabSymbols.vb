'Gadec Engineerings Software (c) 2022
Imports System.Windows.Forms
Imports Autodesk.AutoCAD.Runtime

Public Module RibbonTabSymbols

    ''' <summary>
    ''' The 'Design Center' button.
    ''' <para>Symbol insertion uses the Gadec 'Design Center'.</para>
    ''' </summary>
    <CommandMethod("DESIGNCENTER")>
    Public Sub commandDesignCenter()
        Try
            DesignMethods.DesignCenter(ActiveDocument, "DCsessionSymbols")
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' The 'Coding Symbols' button.
    ''' </summary>
    <CommandMethod("CODESYM")>
    Public Sub commandCodeSymbols()
        Try
            Dim symbolData = DataSetHelper.LoadFromXml("{Support}\SetStandards.xml".Compose).GetTable("Symbols", "Name")
            Dim dialog = New CodingDialog(symbolData)
            If Not dialog.DialogResult = DialogResult.OK Then Exit Sub

            Dim encoder = New SymbolEncoder(ActiveDocument)
            Dim code = dialog.GetSymbolCodeInfo
            code.LoopLineLayerId = LayerHelper.GetLayerIdFromType(ActiveDocument.Database, "Extra")
            encoder.Start(code)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' The 'Check Radius' button.
    ''' </summary>
    <CommandMethod("RADIUSCHECK")>
    Public Sub commandRadiusCheck()
        Try
            DesignMethods.RadiusCheck(ActiveDocument)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' The 'Insert Fire Detectors' button.
    ''' </summary>
    <CommandMethod("DESIGNFIREDETECTORS")>
    Public Sub commandFireDetectors()
        Try
            DesignMethods.DesignFireDetectors(ActiveDocument)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' The 'Insert Wallmount Devices' button.
    ''' </summary>
    <CommandMethod("DESIGNWALLMOUNTDEVICES")>
    Public Sub commandWallmountDevices()
        Try
            DesignMethods.DesignWallmountDevices(ActiveDocument)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' The 'Groupstretch' button.
    ''' </summary>
    <CommandMethod("GROUPSTRETCH", CommandFlags.UsePickSet)>
    Public Sub commandGroupStretch()
        Try
            GroupMethods.Stretch(ActiveDocument)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' The 'Symbol Extract' button.
    ''' </summary>
    <CommandMethod("EXTRACT", CommandFlags.UsePickSet)>
    Public Sub commandExtractDocument()
        Try
            ExtractMethods.ExtractSelection(ActiveDocument)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' The 'Symbol Counter' button.
    ''' </summary>
    <CommandMethod("SYMCOUNTER", CommandFlags.UsePickSet)>
    Public Sub commandSymbolCntr()
        Try
            ExtractMethods.SymbolCounter(ActiveDocument)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' The 'Change Block Scale' button.
    ''' </summary>
    <CommandMethod("CBS", CommandFlags.UsePickSet)>
    Public Sub commandChangeBlockScale()
        Try
            ReferenceMethods.ChangeScale(ActiveDocument)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' The 'Change Block Rotation' button.
    ''' </summary>
    <CommandMethod("CBR", CommandFlags.UsePickSet)>
    Public Sub commandChangeBlockRotation()
        Try
            ReferenceMethods.ChangeRotation(ActiveDocument)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' The 'Redefine Blocks' button.
    ''' </summary>
    <CommandMethod("REDEF_BLOCK")>
    Public Sub commandRedefineBlockDefinitions()
        Try
            ReferenceMethods.RedefineDefinitions(ActiveDocument)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' The 'Replace Blocks' button.
    ''' </summary>
    <CommandMethod("REPLC_BLOCK", CommandFlags.UsePickSet)>
    Public Sub commandsReplaceBlocks()
        Try
            ReferenceMethods.Replace(ActiveDocument)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

End Module
