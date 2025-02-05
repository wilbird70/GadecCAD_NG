'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.Runtime

Public Module AttributeCommands

    ''' <summary>
    ''' Command to unlock the attribute position of a selected attribute (definition or reference) or the 'KKS'-tagged attribute of selected blockreferences.
    ''' </summary>
    <CommandMethod("UAP")>
    Public Sub commandUnlockAttributePosition()
        Try
            AttributeMethods.UnlockPosition(ActiveDocument)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' Command to hide attributes one by one.
    ''' </summary>
    <CommandMethod("ATTR_HIDE")>
    Public Sub commandAttrHide()
        Try
            AttributeMethods.Hide(ActiveDocument)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' Command to show all hidden attributes of a blockreference.
    ''' </summary>
    <CommandMethod("ATTR_SHOWALL")>
    Public Sub commandAttrShowAll()
        Try
            AttributeMethods.ShowAll(ActiveDocument)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

End Module
