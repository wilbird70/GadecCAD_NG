'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.Runtime

Public Module RibbonTabTexts

    ''' <summary>
    ''' The 'Edit Everything' button.
    ''' </summary>
    <CommandMethod("EE")>
    Public Sub commandEditEveryText()
        Try
            TextMethods.EditEveryText(ActiveDocument)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' The 'Coding Texts' button.
    ''' </summary>
    <CommandMethod("CODETEXT")>
    Public Sub commandCodeText()
        Try
            Dim dialog = New InputBoxDialog("CodingText".Translate, Registerizer.UserSetting("CodeAttributeCodeText"))
            If dialog.InputText = "" Then Exit Sub

            Registerizer.UserSetting("CodeAttributeCodeText", dialog.InputText)
            Dim encoder = New TextEncoder(ActiveDocument)
            encoder.Start(dialog.InputText)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' The 'Change Multiple' button.
    ''' </summary>
    <CommandMethod("CM")>
    Public Sub commandChangeMultiple()
        Try
            TextMethods.ChangeMultiple(ActiveDocument)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' The 'Uppercase' button.
    ''' </summary>
    <CommandMethod("UC", CommandFlags.UsePickSet)>
    Public Sub commandUpperCase()
        Try
            TextMethods.ToUpper(ActiveDocument)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' The 'Lowercase' button.
    ''' </summary>
    <CommandMethod("LC", CommandFlags.UsePickSet)>
    Public Sub commandLowerCase()
        Try
            TextMethods.ToLower(ActiveDocument)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' The 'Join Text' button.
    ''' </summary>
    <CommandMethod("JN")>
    Public Sub commandJoinStrings()
        Try
            TextMethods.Join(ActiveDocument)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' The 'Add Before' button.
    ''' </summary>
    <CommandMethod("AB")>
    Public Sub commandAddBefore()
        Try
            TextMethods.AddBefore(ActiveDocument)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' The 'Add After' button.
    ''' </summary>
    <CommandMethod("AA")>
    Public Sub commandAddAfter()
        Try
            TextMethods.AddAfter(ActiveDocument)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' The 'Swap Text' button.
    ''' </summary>
    <CommandMethod("SW")>
    Public Sub commandSwapStrings()
        Try
            TextMethods.Swap(ActiveDocument)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' The 'Copy Text' button.
    ''' </summary>
    <CommandMethod("CT")>
    Public Sub commandCopyStrings()
        Try
            TextMethods.Copy(ActiveDocument)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' The 'Add Number' button.
    ''' </summary>
    <CommandMethod("ADD", CommandFlags.UsePickSet)>
    Public Sub commandMathAdd()
        Try
            TextMethods.AddNumber(ActiveDocument)
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

End Module
