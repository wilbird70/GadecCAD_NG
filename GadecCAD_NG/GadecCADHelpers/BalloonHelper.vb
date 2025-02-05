'Gadec Engineerings Software (c) 2022
Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms
Imports GadecCAD_NG.Extensions

''' <summary>
''' Provides methods for displaying pictureballoons.
''' </summary>
Public Class BalloonHelper

    ''' <summary>
    ''' Shows a Gadec framepreview in a <see cref="PictureBalloon"/>.
    ''' </summary>
    ''' <param name="frameRow">The frame-record.</param>
    ''' <param name="position">The position for the appendix to start.</param>
    ''' <param name="alignment">Alignment of the balloon, left or right from position.</param>
    Public Shared Sub ShowFramePreview(frameRow As DataGridViewRow, position As Drawing.Point, alignment As HorizontalAlignment)
        Static dialog As Object
        dialog?.Dispose()
        dialog = Nothing
        If IsNothing(frameRow) Then Exit Sub

        Dim file = If(ActiveDocument.IsNamedDrawing, "{0}\{1}".Compose(ActiveDocument.GetPath, frameRow.GetString("Filename")), frameRow.GetString("Filename"))
        Dim drawingInUse = DocumentsHelper.GetInuseInfo(file)
        Dim key = frameRow.Cells.Item("Num").Value.ToString
        Dim bitmap = XRecordImagesHelper.Load(file, "{Company}".Compose, "FramePics", key, Drawing.Color.Black)
        Select Case True
            Case drawingInUse?.ByOtherUser
                frameRow.DefaultCellStyle.BackColor = Drawing.Color.OrangeRed
                dialog = New PictureBalloon(Nothing, position, alignment, Color.White, 138, "===", 9)
            Case NotNothing(bitmap)
                dialog = New PictureBalloon(bitmap, position, alignment, Color.White, 138, frameRow.Cells.Item("Filename").Value, 9)
            Case ActiveDocument.NotNamedDrawing Or IO.File.Exists(file)
                Dim dataRow = DirectCast(frameRow.DataBoundItem, DataRowView).Row
                dialog = New TextBalloon(dataRow, position, alignment, Color.White)
            Case Else
                dialog = New PictureBalloon(Nothing, position, alignment, Color.White, 138, "===", 9)
        End Select
        SetFocusOnDataGridView(frameRow.DataGridView)
    End Sub

    ''' <summary>
    ''' Shows a AutoCAD documentpreview in a <see cref="PictureBalloon"/>.
    ''' </summary>
    ''' <param name="fileRow">The file-record.</param>
    ''' <param name="position">The position for the appendix to start.</param>
    ''' <param name="alignment">Alignment of the balloon, left or right from position.</param>
    Public Shared Sub ShowDocumentPreview(fileRow As DataGridViewRow, position As Drawing.Point, alignment As HorizontalAlignment)
        Static dialog As PictureBalloon
        dialog?.Dispose()
        dialog = Nothing
        If IsNothing(fileRow) Then Exit Sub

        Dim fileName = "{0}\{1}".Compose(ActiveDocument.GetPath, fileRow.GetString("Filename"))
        Dim bitmap = LoadDocumentPreview(fileName, Drawing.Color.Empty)
        dialog = New PictureBalloon(bitmap, position, alignment, Color.White, 138, fileName, 9)
        SetFocusOnDataGridView(fileRow.DataGridView)
    End Sub

    ''' <summary>
    ''' Shows a Gadec blockpreview in a <see cref="PictureBalloon"/>.
    ''' </summary>
    ''' <param name="blockName">The name of the block whose image is to be displayed.</param>
    ''' <param name="position">The position for the appendix to start.</param>
    ''' <param name="images">A dictionary of images to choose from.</param>
    Public Shared Sub ShowBlockPreview(blockName As String, position As Drawing.Point, images As Dictionary(Of String, Drawing.Bitmap))
        Static dialog As PictureBalloon
        dialog?.Dispose()
        dialog = Nothing
        If IsNothing(images) Or blockName = "" OrElse Not images.ContainsKey(blockName) Then Exit Sub

        dialog = New PictureBalloon(images(blockName), position, HorizontalAlignment.Left, Color.Blue, 68, blockName, 7)
    End Sub

    'private subs

    ''' <summary>
    ''' Sets the focus to the specified <see cref="DataGridView"/>.
    ''' <para>Note: A re-selection is performed if necessary. Sometimes focusing will erase or shift the selection, requiring reselection.</para>
    ''' </summary>
    ''' <param name="dataGridView">The control to focus on.</param>
    Private Shared Sub SetFocusOnDataGridView(dataGridView As DataGridView)
        Dim selectedRows = dataGridView.SelectedRows
        dataGridView.Focus()
        Dim unlike = False
        For Each dataGridViewRow In selectedRows.ToArray
            If Not dataGridView.SelectedRows.Contains(dataGridViewRow) Then unlike = True
        Next
        If unlike Then
            dataGridView.ClearSelection()
            For Each dataGridViewRow In selectedRows.ToArray
                dataGridViewRow.Selected = True
            Next
        End If
    End Sub

    'private functions

    ''' <summary>
    ''' Loads the AutoCAD documentpreview from the specified file.
    ''' </summary>
    ''' <param name="fileName">The fullname of the AutoCAD document.</param>
    ''' <param name="transparantColor">The color that will be converted to transparent.</param>
    ''' <returns></returns>
    Private Shared Function LoadDocumentPreview(fileName As String, transparantColor As Drawing.Color) As Drawing.Bitmap
        Try
            If Not IO.File.Exists(fileName) Then Return Nothing

            Using fileStream = New IO.FileStream(fileName, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.ReadWrite)
                Dim binaryReader = New IO.BinaryReader(fileStream)
                fileStream.Seek(&HD, IO.SeekOrigin.Begin)
                fileStream.Seek(&H14 + binaryReader.ReadInt32(), IO.SeekOrigin.Begin)
                Dim binarySize As Byte = binaryReader.ReadByte()
                If binarySize <= 1 Then Return Nothing

                For i As Short = 1 To binarySize
                    Dim imageCode = binaryReader.ReadByte()
                    Dim imageHeaderStart = binaryReader.ReadInt32()
                    Dim imageHeaderSize = binaryReader.ReadInt32()
                    Select Case imageCode
                        Case 6      'ACAD2013 and later
                            Dim bitmap = GetPngPreview(fileStream, imageHeaderStart)
                            bitmap.MakeTransparent(transparantColor)
                            Return bitmap
                        Case 2      'ACAD2012 and previous
                            Dim bitmap = GetBmpPreview(binaryReader, fileStream, imageHeaderStart, imageHeaderSize)
                            bitmap.MakeTransparent(transparantColor)
                            Return bitmap
                        Case 3      'no preview available
                            Return Nothing
                    End Select
                Next
            End Using
            Return Nothing
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' Retrieves the bitmap from the png-formatted AutoCAD documentpreview.
    ''' <para>Note: For AutoCAD 2013 and later documentformats.</para>
    ''' </summary>
    ''' <param name="stream"></param>
    ''' <param name="headerStart"></param>
    ''' <returns></returns>
    Private Shared Function GetPngPreview(stream As IO.FileStream, headerStart As Integer) As Drawing.Bitmap
        stream.Seek(headerStart, IO.SeekOrigin.Begin)
        Using memoryStream As New IO.MemoryStream
            stream.CopyTo(memoryStream, headerStart)
            Dim bitmap = New Drawing.Bitmap(memoryStream)
            Return bitmap
        End Using
    End Function

    ''' <summary>
    ''' Retrieves the bitmap from the bmp-formatted AutoCAD documentpreview.
    ''' <para>Note: For AutoCAD 2012 and earlier documentformats.</para>
    ''' </summary>
    ''' <param name="reader"></param>
    ''' <param name="stream"></param>
    ''' <param name="headerStart"></param>
    ''' <param name="headerSize"></param>
    ''' <returns></returns>
    Private Shared Function GetBmpPreview(reader As IO.BinaryReader, stream As IO.FileStream, headerStart As Integer, headerSize As Integer) As Drawing.Bitmap
        reader.ReadBytes(&HE)
        Dim bitCount = reader.ReadUInt16()
        reader.ReadBytes(4)
        Dim sizeImage = reader.ReadUInt32()
        stream.Seek(headerStart, IO.SeekOrigin.Begin)
        Dim bitmapBuffer = reader.ReadBytes(headerSize)
        Dim clearTabSize = CUInt(If(bitCount < 9, 4 * (2 ^ bitCount), 0))
        Using memoryStream = New IO.MemoryStream
            Dim binaryWriter = New IO.BinaryWriter(memoryStream)
            With binaryWriter
                .Write(CUShort(&H4D42))
                .Write(54UI + clearTabSize + sizeImage)
                .Write(New UShort())
                .Write(New UShort())
                .Write(54UI + clearTabSize)
                .Write(bitmapBuffer)
            End With
            Dim bitmap = New Drawing.Bitmap(memoryStream)
            Return bitmap
        End Using
    End Function

End Class
