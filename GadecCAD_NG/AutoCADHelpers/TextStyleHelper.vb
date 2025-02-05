'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.DatabaseServices
Imports GadecCAD_NG.Extensions

''' <summary>
''' Provide a methods for textstyles.
''' </summary>
Public Class TextStyleHelper

    ''' <summary>
    ''' Gets an objectid of a textstyle with the specified textfont.
    ''' <para>If the textstyle is not available in the drawing, it tries to create it with the specified textfont-file (shx).</para>
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="textFont">The name of the textfont.</param>
    ''' <returns>The objectid of the textstyle.</returns>
    Public Shared Function GetTextStyleId(database As Database, textFont As String) As ObjectId
        Dim output = ObjectId.Null
        Using tr = database.TransactionManager.StartTransaction
            Dim textStyleTable = tr.GetTextStyleTable(database.TextStyleTableId, OpenMode.ForWrite)
            Select Case textStyleTable.Has(textFont)
                Case True
                    output = textStyleTable(textFont)
                    Dim textStyle = tr.GetTextStyleTableRecord(output, OpenMode.ForWrite)
                    textStyle.FileName = "{0}.shx".Compose(textFont)
                    textStyle.TextSize = 0
                Case Else
                    Dim textStyle = New TextStyleTableRecord With {.Name = textFont, .FileName = "{0}.shx".Compose(textFont), .TextSize = 0}
                    textStyleTable.Add(textStyle)
                    tr.AddNewlyCreatedDBObject(textStyle, True)
                    output = textStyle.ObjectId
            End Select
            tr.Commit()
        End Using
        Return output
    End Function

End Class
