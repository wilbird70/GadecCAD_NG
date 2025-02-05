'Gadec Engineerings Software (c) 2022
Imports System.Windows
Imports PdfSharp.Drawing
Imports PdfSharp.Pdf

''' <summary>
''' Provides methods to modify PDF files.
''' </summary>
Public Class PdfSharpHelper

    'subs

    ''' <summary>
    ''' Adds a field to a page of the PDF file to allow users to digitally sign the document.
    ''' </summary>
    ''' <param name="pdfDocument">The PDF document.</param>
    ''' <param name="page">The zero-based page number.</param>
    ''' <param name="rectangles">A list of rectangles representing the signfields.</param>
    ''' <param name="description">The description entry in the subject of an e-mail, which is created after using a signfield (digitally signing the document).</param>
    Public Shared Sub AddSignFields(ByRef pdfDocument As PdfDocument, page As Integer, rectangles As Rect(), description As String)
        Dim graphics = XGraphics.FromPdfPage(pdfDocument.Pages(page))
        Dim subject = "PdfSubject".Translate(description)
        Dim body = "PdfBody".Translate(description)
        Dim code = "this.mailDoc({" & "bUI: true, cTo: {Q}{Q}, cSubject: {Q}{0}{Q}, cMsg: {Q}{1}{Q}".Compose(subject, body) & "});{C}".Compose
        Dim javaScript = New PdfDictionary(pdfDocument)
        javaScript.Elements.Add("/S", New PdfName("/JavaScript"))
        javaScript.Elements.Add("/JS", New PdfString(code))
        pdfDocument.Internals.AddObject(javaScript)

        Dim number = 1
        For Each rect In rectangles
            Dim left = XUnit.FromMillimeter(rect.X).Point
            Dim top = XUnit.FromMillimeter(rect.Y).Point
            Dim width = XUnit.FromMillimeter(rect.Width).Point
            Dim height = XUnit.FromMillimeter(rect.Height).Point
            Dim pageHeight = pdfDocument.Pages(0).Height.Point
            Dim rectangle = New PdfRectangle(New XPoint(left, pageHeight - top), New XPoint(left + width, pageHeight - top - height))
            Dim signature = New PdfSignatureField(pdfDocument, page, number, rectangle, javaScript)
            pdfDocument.Pages(page).Annotations.Add(signature)
            DrawRectangle(graphics, New XPoint(left, top), New XPoint(left + width, top + height))
            number += 1
        Next
    End Sub

    ''' <summary>
    ''' Adds the PDF files to the document.
    ''' </summary>
    ''' <param name="pdfDocument">The PDF document.</param>
    ''' <param name="files">A list of fullnames of PDF files.</param>
    Public Shared Sub AddPdfFiles(ByRef pdfDocument As PdfDocument, files As String())
        Dim missingFiles = New List(Of String)
        For Each file In files
            If Not System.IO.File.Exists(file) Then missingFiles.Add(System.IO.Path.GetFileName(file)) : Continue For

            Try
                Using sourceDoc = IO.PdfReader.Open(file, IO.PdfDocumentOpenMode.Import)
                    For i = 0 To sourceDoc.PageCount - 1
                        pdfDocument.AddPage(sourceDoc.Pages(i))
                    Next
                End Using
            Catch ex As Exception
                missingFiles.Add(System.IO.Path.GetFileName(file))
            End Try
        Next
        If missingFiles.Count > 0 Then
            MessageBoxInfo("File error{2L}The following files could not included in the package:{2L}{0}".NotYetTranslated(String.Join(vbLf, missingFiles)))
        End If
    End Sub

    'functions

    ''' <summary>
    ''' Merges PDF files into a new PDF document.
    ''' </summary>
    ''' <param name="fileNames">A list of fullnames of PDF files.</param>
    ''' <returns>The new PDF document.</returns>
    Public Shared Function MergePdfFiles(fileNames As String()) As PdfDocument
        Dim output = New PdfDocument
        Dim missingFiles = New List(Of String)
        For Each file In fileNames
            If Not System.IO.File.Exists(file) Then missingFiles.Add(System.IO.Path.GetFileName(file)) : Continue For

            Try
                Using sourceDoc = IO.PdfReader.Open(file, IO.PdfDocumentOpenMode.Import)
                    For i = 0 To sourceDoc.PageCount - 1
                        output.AddPage(sourceDoc.Pages(i))
                    Next
                End Using
                If FileSystemHelper.FileNotLocked(file) Then FileSystemHelper.DeleteFile(file)
            Catch ex As Exception
                missingFiles.Add(System.IO.Path.GetFileName(file))
            End Try
        Next
        If missingFiles.Count > 0 Then
            MessageBoxInfo("File error{2L}The following files could not included in the package:{2L}{0}".NotYetTranslated(String.Join(vbLf, missingFiles)))
        End If
        Return output
    End Function

    ''' <summary>
    ''' Gets the number of pages of a PDF file.
    ''' </summary>
    ''' <param name="fileName">The fullname of the PDF file.</param>
    ''' <returns>The number of pages.</returns>
    Public Shared Function GetNumberOfPages(fileName As String) As Integer
        If Not System.IO.File.Exists(fileName) Then Return 0

        Try
            Using sourceDoc = IO.PdfReader.Open(fileName, IO.PdfDocumentOpenMode.Import)
                Return sourceDoc.PageCount
            End Using
        Catch ex As Exception
            Return 0
        End Try
    End Function

    'private subs

    ''' <summary>
    ''' Draws a rectangle on a page of a PDF file.
    ''' </summary>
    ''' <param name="graphics">The graphics of the page.</param>
    ''' <param name="point1">Topleft corner.</param>
    ''' <param name="point2">Bottomright corner.</param>
    Private Shared Sub DrawRectangle(ByVal graphics As XGraphics, point1 As XPoint, point2 As XPoint)
        Dim xRect = New XRect(point1, point2)
        Dim xPen = New XPen(XColors.Black, 1)
        graphics.DrawRectangle(xPen, xRect)
    End Sub

End Class
