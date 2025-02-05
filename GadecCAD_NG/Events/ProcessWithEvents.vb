'Gadec Engineerings Software (c) 2022

''' <summary>
''' <para><see cref="ProcessWithEvents"/> can start a new document and removes temporary files (tmp) when the document is closed.</para>
''' </summary>
Public Class ProcessWithEvents
    ''' <summary>
    ''' Contains the fullname of the document.
    ''' </summary>
    Private ReadOnly _fileName As String
    ''' <summary>
    ''' Contains the date and time of the document.
    ''' </summary>
    Private ReadOnly _lastWriteTime As Date

    ''' <summary>
    ''' Initializes a new instance of <see cref="ProcessWithEvents"/>.
    ''' <para><see cref="ProcessWithEvents"/> can start a new document and removes temporary files (tmp) when the document is closed.</para>
    ''' </summary>
    ''' <param name="fileName">The fullname of the document to open.</param>
    Public Sub New(fileName As String)
        Dim criterion = Function() IO.File.Exists(fileName)
        Dim dialog = New WaitUntilDialog(criterion, 5)
        If Not dialog.Successfully Then Exit Sub

        _fileName = fileName
        _lastWriteTime = IO.File.GetLastWriteTime(fileName)
        Dim process = ProcessHelper.StartDocument(fileName)
        process.EnableRaisingEvents = True
        AddHandler process.Exited, AddressOf ProcessExitedEventHandler
    End Sub

    'private eventhandlers

    ''' <summary>
    ''' EventHandler for the event that occurs when the document is closed.
    ''' <para>This handler removes the temporary files (*.tmp).</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ProcessExitedEventHandler(sender As Object, e As EventArgs)
        Dim files = IO.Directory.GetFiles(IO.Path.GetDirectoryName(_fileName), "*.tmp").ToList
        files.ForEach(Sub(file) If IO.File.GetLastWriteTime(file) = _lastWriteTime Then IO.File.Delete(file))
        Dim process = DirectCast(sender, Process)
        RemoveHandler process.Exited, AddressOf ProcessExitedEventHandler
        process.Dispose()
    End Sub

End Class
