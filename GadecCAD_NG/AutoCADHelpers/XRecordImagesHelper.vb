'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports GadecCAD_NG.Extensions

''' <summary>
''' Provides methods for loading and saving images from/to the application's xrecords.
''' </summary>
Public Class XRecordImagesHelper

    'subs

    ''' <summary>
    ''' Saves an image to an xrecord in the application's dictionary.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="company">The name of the company.</param>
    ''' <param name="application">The name of the application.</param>
    ''' <param name="entryName">The entryname of the xrecord.</param>
    ''' <param name="image">The image to store.</param>
    Public Shared Sub Save(document As Document, company As String, application As String, entryName As String, image As Drawing.Bitmap)
        Using document.LockDocument
            Save(document.Database, company, application, New Dictionary(Of String, Drawing.Bitmap) From {{entryName, image}})
        End Using
    End Sub

    ''' <summary>
    ''' Saves images to xrecords in the application's dictionary.
    ''' <para>Note: The keys of the dictionary become the entrynames of the xrecords.</para>
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="company">The name of the company.</param>
    ''' <param name="application">The name of the application.</param>
    ''' <param name="images">The images to store.</param>
    Public Shared Sub Save(document As Document, company As String, application As String, images As Dictionary(Of String, Drawing.Bitmap))
        Using document.LockDocument
            Save(document.Database, company, application, images)
        End Using
    End Sub

    ''' <summary>
    ''' Saves an image to an xrecord in the application's dictionary.
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="company">The name of the company.</param>
    ''' <param name="application">The name of the application.</param>
    ''' <param name="entryName">The entryname of the xrecord.</param>
    ''' <param name="image">The image to store.</param>
    Public Shared Sub Save(database As Database, company As String, application As String, entryName As String, image As Drawing.Bitmap)
        Save(database, company, application, New Dictionary(Of String, Drawing.Bitmap) From {{entryName, image}})
    End Sub

    ''' <summary>
    ''' Saves images to xrecords in the application's dictionary.
    ''' <para>Note: The keys of the dictionary become the entrynames of the xrecords.</para>
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="company">The name of the company.</param>
    ''' <param name="application">The name of the application.</param>
    ''' <param name="images">The images to store.</param>
    Public Shared Sub Save(database As Database, company As String, application As String, images As Dictionary(Of String, Drawing.Bitmap))
        Dim dictionaryId = XRecordHelper.GetDictionaryId(database, company, application, True)
        If dictionaryId.IsNull Then Exit Sub

        Using tr = database.TransactionManager.StartTransaction()
            Dim dictionary = tr.GetDBDictionary(dictionaryId, OpenMode.ForWrite)
            For Each pair In images
                Using otputStream = New IO.MemoryStream()
                    Using inputStream = New IO.MemoryStream()
                        pair.Value.Save(inputStream, System.Drawing.Imaging.ImageFormat.Bmp)
                        Dim startBuffer = inputStream.ToArray
                        Using gZip = New IO.Compression.GZipStream(otputStream, IO.Compression.CompressionMode.Compress)
                            gZip.Write(startBuffer, 0, startBuffer.Length)
                        End Using
                        Dim zipBuffer = otputStream.ToArray()
                        Dim resultBuffer = New ResultBuffer()
                        Dim maxChunkSize = 127
                        Dim bytesConsumed = 0
                        While bytesConsumed < zipBuffer.Length
                            Dim bytesToRead = zipBuffer.Length - bytesConsumed
                            Dim count = If(bytesToRead < maxChunkSize, bytesToRead, maxChunkSize)
                            Dim chunk = New Byte(count - 1) {}
                            Array.Copy(zipBuffer, bytesConsumed, chunk, 0, count)
                            resultBuffer.Add(New TypedValue(DxfCode.BinaryChunk, chunk))
                            bytesConsumed += count
                        End While
                        Dim xRecord = New Xrecord With {.Data = resultBuffer}
                        dictionary.SetAt(pair.Key, xRecord)
                        tr.AddNewlyCreatedDBObject(xRecord, True)
                    End Using
                End Using
            Next
            tr.Commit()
        End Using
    End Sub

    'functions

    ''' <summary>
    ''' Loads the image from the xrecord in the application's dictionary on the specified entryname of the specified document-file.
    ''' </summary>
    ''' <param name="fileName">The fullname of the source-file.</param>
    ''' <param name="company">The name of the company.</param>
    ''' <param name="application">The name of the application.</param>
    ''' <param name="entryName">The entryname of the xrecord.</param>
    ''' <param name="transparantColor">The color to make transparant.</param>
    ''' <returns>The image.</returns>
    Public Shared Function Load(fileName As String, company As String, application As String, entryName As String, transparantColor As Drawing.Color) As Drawing.Bitmap
        Dim images = Load(fileName, company, application, {entryName}, transparantColor)
        If Not images.ContainsKey(entryName) Then Return Nothing

        Return images(entryName)
    End Function

    ''' <summary>
    ''' Loads images from the xrecords in the application's dictionary on the specified entrynames of the specified document-file.
    ''' <para>Note: The entrynames of the xrecords become the keys of the dictionary.</para>
    ''' </summary>
    ''' <param name="fileName">The fullname of the source-file.</param>
    ''' <param name="company">The name of the company.</param>
    ''' <param name="application">The name of the application.</param>
    ''' <param name="entryNames">A list of entrynames of xrecords.</param>
    ''' <param name="transparantColor">The color to make transparant.</param>
    ''' <returns>The images.</returns>
    Public Shared Function Load(fileName As String, company As String, application As String, entryNames As String(), transparantColor As Drawing.Color) As Dictionary(Of String, Drawing.Bitmap)
        Dim output = New Dictionary(Of String, Drawing.Bitmap)
        Dim documents = DocumentsHelper.GetOpenDocuments()
        Dim db As Database = Nothing
        Try
            Select Case True
                Case documents.ContainsKey(fileName)
                    db = documents(fileName).Database
                Case IO.File.Exists(fileName)
                    db = New Database(False, True)
                    db.ReadDwgFile(fileName, FileOpenMode.OpenForReadAndAllShare, True, "")
                Case Else : Return New Dictionary(Of String, Drawing.Bitmap)
            End Select
            Dim dictionaryId = XRecordHelper.GetDictionaryId(db, company, application, False)
            If dictionaryId.IsNull Then
                entryNames.ToList.ForEach(Sub(name) output.TryAdd(name, Nothing))
                Return output
            End If

            Using tr = db.TransactionManager.StartTransaction()
                Dim dictionary = tr.GetDBDictionary(dictionaryId)
                For Each name In entryNames
                    If Not dictionary.Contains(name) Then output.TryAdd(name, Nothing) : Continue For

                    Dim bitmap As Drawing.Bitmap = Nothing
                    Using inputStream = New IO.MemoryStream()
                        Using unzipStream = New IO.MemoryStream()
                            Dim xRecord = tr.GetXrecord(dictionary.GetAt(name))
                            For Each tv In xRecord.Data
                                If tv.TypeCode = CInt(DxfCode.BinaryChunk) Then
                                    Dim buf = CType(tv.Value, Byte())
                                    inputStream.Write(buf, 0, buf.Length)
                                End If
                            Next
                            inputStream.Position = 0
                            Using gZip = New IO.Compression.GZipStream(inputStream, IO.Compression.CompressionMode.Decompress)
                                gZip.CopyTo(unzipStream)
                            End Using
                            unzipStream.Position = 0
                            Using outputStream = New IO.MemoryStream(unzipStream.ToArray)
                                bitmap = Drawing.Image.FromStream(outputStream)
                            End Using
                        End Using
                    End Using
                    If Not transparantColor = Drawing.Color.Empty Then bitmap?.MakeTransparent(transparantColor)
                    output.TryAdd(name, bitmap)
                Next
                tr.Commit()
            End Using
            Return output
        Catch ex As System.Exception
            Return New Dictionary(Of String, Drawing.Bitmap)
        Finally
            db?.Dispose()
        End Try
    End Function

End Class
