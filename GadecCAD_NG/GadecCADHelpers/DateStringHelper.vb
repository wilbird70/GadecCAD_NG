'Gadec Engineerings Software (c) 2022

''' <summary>
''' Provides a method to convert an old used date format to the new format.
''' </summary>
Public Class DateStringHelper

    ''' <summary>
    ''' Converts an old used date format to the new format.
    ''' </summary>
    ''' <param name="dateString">A datestring.</param>
    ''' <returns>The date in the new date format.</returns>
    Public Shared Function Convert(dateString As String) As String
        Dim output = dateString.Replace(".", "-").Trim
        Select Case True
            Case output = ""
            Case output = " "
            Case IsDate(output)
            Case Else : output = "{0}-{1}-{2}".Compose(dateString.MidString(1, 2), dateString.MidString(3, 2), dateString.MidString(5))
        End Select
        Select Case IsDate(output)
            Case True : Return Format(CDate(output), "dd-MM-yyyy")
            Case Else : Return output
        End Select
    End Function

End Class
