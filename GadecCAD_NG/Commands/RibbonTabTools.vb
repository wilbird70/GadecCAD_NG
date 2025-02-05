'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.Runtime

Public Module RibbonTabTools

    ''' <summary>
    ''' The 'Calculate Area' button.
    ''' </summary>
    <CommandMethod("CALCAREA", CommandFlags.UsePickSet)>
    Public Sub commandCalcArea()
        Try
            PolylineMethods.ShowAreaSize(ActiveDocument)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' The 'Reroot Xref-path' button.
    ''' </summary>
    <CommandMethod("CXP")>
    Public Sub commandChangeXRefPathMethod()
        Try
            XrefHelper.ReRootPaths(ActiveDocument)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' The 'Revision Cloud' button.
    ''' </summary>
    <CommandMethod("RV")>
    Public Sub commandRevCloud()
        Try
            PolylineMethods.CreateRevisionCloud(ActiveDocument)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' The 'Deepcolor' button.
    ''' </summary>
    <CommandMethod("DEEPCOLORING", CommandFlags.UsePickSet)>
    Public Sub commandDeepColoring()
        Try
            DeepColoringMethod.Start(ActiveDocument)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' The 'Wiring Duct' button.
    ''' </summary>
    <CommandMethod("WD")>
    Public Sub commandWiringDuct()
        Try
            WiringDuctMethod.Draw(ActiveDocument)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

End Module
